using System.Reflection;

namespace Galaxxis.Architecture.Tests;

/// <summary>
/// Verifies the module dependency rules (documented in CLAUDE.md and each module's README.md)
/// against the compiled assemblies.
/// </summary>
/// <remarks>
/// The C# compiler only records a reference to another assembly when code actually uses it.
/// These tests therefore fail the moment someone writes code against a forbidden module, no matter
/// how the project reference got there.
/// </remarks>
public class ModuleDependencyTests
{
    /// <summary>
    /// For each module: the other Galaxxis modules it is allowed to use.
    /// Everything not listed here is forbidden; above all, no module may use the web application.
    /// </summary>
    public static TheoryData<string, string[]> AllowedModuleReferences => new()
    {
        { "Galaxxis.Domain", [] },
        { "Galaxxis.Persistence", ["Galaxxis.Domain"] },
        { "Galaxxis.Application", ["Galaxxis.Domain", "Galaxxis.Persistence"] },
        { "Galaxxis.Engine", ["Galaxxis.Domain", "Galaxxis.Persistence"] },

        // The web application calls Application services and shows Domain types (resources, buildings, races).
        // Persistence and Engine are only used for registration (and Identity's account types).
        { "Galaxxis.Web", ["Galaxxis.Domain", "Galaxxis.Persistence", "Galaxxis.Application", "Galaxxis.Engine"] },
    };

    [Theory]
    [MemberData(nameof(AllowedModuleReferences))]
    public void Module_uses_only_the_Galaxxis_modules_it_is_allowed_to(string module, string[] allowed)
    {
        IEnumerable<string> usedGalaxxisModules = ReferencedAssemblyNames(module)
            .Where(name => name.StartsWith("Galaxxis.", StringComparison.Ordinal));

        IEnumerable<string> forbidden = usedGalaxxisModules.Except(allowed);

        Assert.Empty(forbidden);
    }

    [Theory]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.Extensions.")]
    [InlineData("Microsoft.AspNetCore.")]
    public void Domain_does_not_use_infrastructure_packages(string forbiddenPrefix)
    {
        // The game rules must stay pure: no database, no hosting, no configuration, no web.
        IEnumerable<string> forbidden = ReferencedAssemblyNames("Galaxxis.Domain")
            .Where(name => name.StartsWith(forbiddenPrefix, StringComparison.Ordinal));

        Assert.Empty(forbidden);
    }

    private static IEnumerable<string> ReferencedAssemblyNames(string module)
    {
        return Assembly.Load(new AssemblyName(module))
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty);
    }
}
