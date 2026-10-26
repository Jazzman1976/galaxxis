using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// The exclusive right of this engine instance to process one universe, backed by a SQL Server application lock.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why.</b> If two engine instances processed the same universe (a second web server, an overlapping deployment),
/// events could run twice. Before processing a universe, an instance must hold its lease. A second instance simply
/// does not get it and waits as a hot standby; when the first instance stops, its lock is released and the second one
/// takes over within one poll interval.
/// </para>
/// <para>
/// <b>How.</b> <c>sp_getapplock</c> with the lock owner "Session" holds the lock for as long as the database connection
/// stays open. The lease therefore keeps its own dedicated connection open. If that connection breaks (for example
/// during an Azure SQL failover), SQL Server releases the lock; <see cref="IsStillHeldAsync"/> notices this before
/// every processing round, so an instance never continues without the lock.
/// </para>
/// </remarks>
internal sealed class UniverseLease : IAsyncDisposable
{
    private readonly SqlConnection _connection;
    private readonly string _resource;
    private readonly ILogger _logger;

    private UniverseLease(int universeId, SqlConnection connection, string resource, ILogger logger)
    {
        UniverseId = universeId;
        _connection = connection;
        _resource = resource;
        _logger = logger;
    }

    /// <summary>The universe this lease is for.</summary>
    public int UniverseId { get; }

    /// <summary>Tries to acquire the lease without waiting.</summary>
    /// <param name="connectionString">Connection string of the game database.</param>
    /// <param name="universeId">The universe to lease.</param>
    /// <param name="logger">Logger for lease changes.</param>
    /// <param name="cancellationToken">Cancels the attempt.</param>
    /// <returns>The lease, or <see langword="null"/> if another instance holds it.</returns>
    public static async Task<UniverseLease?> TryAcquireAsync(
        string connectionString, int universeId, ILogger logger, CancellationToken cancellationToken)
    {
        string resource = $"galaxxis-universe-{universeId}";
        SqlConnection connection = new(connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);

            await using SqlCommand command = connection.CreateCommand();
            command.CommandText = "sp_getapplock";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Resource", resource);
            command.Parameters.AddWithValue("@LockMode", "Exclusive");
            command.Parameters.AddWithValue("@LockOwner", "Session");
            command.Parameters.AddWithValue("@LockTimeout", 0);
            SqlParameter result = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
            result.Direction = ParameterDirection.ReturnValue;

            await command.ExecuteNonQueryAsync(cancellationToken);

            // 0 = granted, 1 = granted after waiting; negative values = not granted (timeout, deadlock, error).
            if ((int)result.Value >= 0)
            {
                logger.LogInformation("This engine instance now processes universe {UniverseId}.", universeId);
                return new UniverseLease(universeId, connection, resource, logger);
            }
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }

        await connection.DisposeAsync();
        return null;
    }

    /// <summary>Checks that the lock is still held on the open connection.</summary>
    /// <param name="cancellationToken">Cancels the check.</param>
    /// <returns><see langword="true"/> if this instance still holds the universe.</returns>
    public async Task<bool> IsStillHeldAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != ConnectionState.Open)
        {
            return false;
        }

        try
        {
            await using SqlCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT APPLOCK_MODE('public', @Resource, 'Session')";
            command.Parameters.AddWithValue("@Resource", _resource);
            object? mode = await command.ExecuteScalarAsync(cancellationToken);
            return string.Equals(mode as string, "Exclusive", StringComparison.Ordinal);
        }
        catch (Exception exception) when (exception is SqlException or InvalidOperationException)
        {
            _logger.LogWarning(exception, "The lease on universe {UniverseId} was lost.", UniverseId);
            return false;
        }
    }

    /// <summary>Releases the lock by closing the connection.</summary>
    /// <returns>A task that completes when the connection is closed.</returns>
    public async ValueTask DisposeAsync()
    {
        // Closing the session releases a session-owned application lock.
        await _connection.DisposeAsync();
        _logger.LogInformation("This engine instance stopped processing universe {UniverseId}.", UniverseId);
    }
}
