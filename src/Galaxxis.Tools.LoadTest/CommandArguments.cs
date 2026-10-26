using System.Globalization;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// The arguments after the command name: named options (<c>--players 1000</c>) and at most one plain value
/// (the universe id in <c>verify 3</c>).
/// </summary>
/// <remarks>
/// Same format as in the universe generator tool. Each tool keeps its own small copy, so the tools stay independent
/// hosts without a shared helper project.
/// </remarks>
internal sealed class CommandArguments
{
    private readonly Dictionary<string, string> _options = new(StringComparer.OrdinalIgnoreCase);
    private string? _value;

    /// <summary>Reads the arguments.</summary>
    /// <param name="arguments">The command line arguments without the command name.</param>
    /// <returns>The parsed arguments.</returns>
    /// <exception cref="ArgumentException">An option has no value, or there is more than one plain value.</exception>
    public static CommandArguments Parse(IEnumerable<string> arguments)
    {
        CommandArguments result = new();
        using IEnumerator<string> enumerator = arguments.GetEnumerator();

        while (enumerator.MoveNext())
        {
            string argument = enumerator.Current;

            if (argument.StartsWith("--", StringComparison.Ordinal))
            {
                string name = argument[2..];
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentException($"The option --{name} needs a value.");
                }

                result._options[name] = enumerator.Current;
            }
            else if (result._value is null)
            {
                result._value = argument;
            }
            else
            {
                throw new ArgumentException($"Unexpected argument \"{argument}\".");
            }
        }

        return result;
    }

    /// <summary>Gets a text option.</summary>
    /// <param name="name">Option name without dashes.</param>
    /// <returns>The value, or <see langword="null"/> if the option was not given.</returns>
    public string? Text(string name) => _options.GetValueOrDefault(name);

    /// <summary>Gets a whole-number option.</summary>
    /// <param name="name">Option name without dashes.</param>
    /// <returns>The value, or <see langword="null"/> if the option was not given.</returns>
    /// <exception cref="FormatException">The value is not a whole number.</exception>
    public int? Number(string name) =>
        Text(name) is string text
            ? int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)
                ? number
                : throw new FormatException($"--{name} must be a whole number, but was \"{text}\".")
            : null;

    /// <summary>Gets a duration option in the format hh:mm:ss.</summary>
    /// <param name="name">Option name without dashes.</param>
    /// <returns>The value, or <see langword="null"/> if the option was not given.</returns>
    /// <exception cref="FormatException">The value is not a duration.</exception>
    public TimeSpan? Duration(string name) =>
        Text(name) is string text
            ? TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out TimeSpan duration)
                ? duration
                : throw new FormatException($"--{name} must be a duration like 00:30:00, but was \"{text}\".")
            : null;

    /// <summary>Gets the plain value as a universe id.</summary>
    /// <returns>The id.</returns>
    /// <exception cref="ArgumentException">No id was given or it is not a number.</exception>
    public int RequireId() =>
        int.TryParse(_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id)
            ? id
            : throw new ArgumentException("Please give the universe id, for example: verify 3");
}
