using Galaxxis.Application;
using Galaxxis.Engine;
using Galaxxis.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Galaxxis.Architecture.Tests;

/// <summary>
/// Guards the repository rule: every git-ignored file a program needs has a checked-in copy template named
/// "&lt;original file name&gt;.template" that works after removing the suffix and adjusting the values.
/// </summary>
public class ConfigurationTemplateTests
{
    private const string LocalSettingsTemplate = "appsettings.Local.json.template";

    [Fact]
    public void The_local_settings_template_contains_every_required_setting()
    {
        IConfiguration template = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(RepositoryRoot(), LocalSettingsTemplate), optional: false, reloadOnChange: false)
            .Build();

        // Register every module the way a host does and run the same startup validation. If a module gains a new
        // required setting, this fails until the template is extended.
        ServiceCollection services = new();
        services.AddLogging();
        services.AddGalaxxisPersistence(template);
        services.AddGalaxxisApplication(template);
        services.AddGalaxxisEngine(template);
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IStartupValidator>().Validate();

        // BootstrapAdmin is only validated when used, so check its keys directly.
        IConfigurationSection admin = template.GetSection("BootstrapAdmin");
        Assert.All(
            new[] { "Email", "UserName", "Password", "FirstName", "LastName" },
            key => Assert.True(admin.GetSection(key).Exists(), $"{LocalSettingsTemplate} is missing BootstrapAdmin:{key}."));
    }

    [Fact]
    public void The_local_settings_template_contains_no_password()
    {
        IConfiguration template = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(RepositoryRoot(), LocalSettingsTemplate), optional: false, reloadOnChange: false)
            .Build();

        Assert.Equal(string.Empty, template["BootstrapAdmin:Password"]);
        Assert.DoesNotContain("Password=", template["ConnectionStrings:Game"], StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password=", template["ConnectionStrings:Accounts"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Every_ignored_configuration_file_has_a_template()
    {
        // In the root .gitignore, build output and IDE state are ignored with wildcards or folder patterns.
        // Every other entry is a single file a program needs, and therefore requires a template next to it.
        string root = RepositoryRoot();
        IEnumerable<string> requiredIgnoredFiles = File.ReadAllLines(Path.Combine(root, ".gitignore"))
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#') && !line.StartsWith('!'))
            .Where(line => !line.Contains('*') && !line.EndsWith('/'));

        Assert.All(requiredIgnoredFiles, file =>
            Assert.True(File.Exists(Path.Combine(root, file + ".template")), $"{file} is git-ignored but has no {file}.template."));
    }

    /// <summary>Finds the repository root by walking up from the test output folder to the folder with global.json.</summary>
    private static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root (folder with global.json) not found.");
    }
}
