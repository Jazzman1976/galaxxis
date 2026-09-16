using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Persistence.Accounts;

/// <summary>
/// A login account. Extends the ASP.NET Core Identity user with the person's name.
/// </summary>
/// <remarks>
/// A login account is not a player: the player in a universe lives in the game database and points to this
/// account by its <c>Id</c> (see <c>Player.AccountId</c>).
/// </remarks>
public sealed class AppUser : IdentityUser
{
    /// <summary>First name, optional.</summary>
    public string? FirstName { get; set; }

    /// <summary>Last name, optional.</summary>
    public string? LastName { get; set; }
}

/// <summary>A role that grants permissions, for example <see cref="AccountRoles.Admin"/>.</summary>
public sealed class AppRole : IdentityRole
{
    /// <summary>Creates an empty role, as required by Identity and EF Core.</summary>
    public AppRole()
    {
    }

    /// <summary>Creates a role with the given name.</summary>
    /// <param name="roleName">The role name.</param>
    public AppRole(string roleName) : base(roleName)
    {
    }
}

/// <summary>The names of the roles Galaxxis uses.</summary>
public static class AccountRoles
{
    /// <summary>Administrators may manage universes (pause, resume, generate).</summary>
    public const string Admin = "Admin";
}

/// <summary>
/// Database context of the login accounts (database <c>GalaxxisAccounts</c>), managed by ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// Kept in a separate database on purpose: the game never depends on how people log in, and the login system
/// could be replaced (for example by an external identity provider) without touching game data.
/// Migrations live in <c>Accounts/Migrations</c>.
/// </remarks>
/// <param name="options">Connection and provider settings, supplied by dependency injection.</param>
public sealed class AccountDbContext(DbContextOptions<AccountDbContext> options)
    : IdentityDbContext<AppUser, AppRole, string>(options)
{
    /// <summary>Configures the Identity tables and the additional user columns.</summary>
    /// <param name="builder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(user =>
        {
            user.Property(u => u.FirstName).HasMaxLength(100);
            user.Property(u => u.LastName).HasMaxLength(100);
        });
    }
}
