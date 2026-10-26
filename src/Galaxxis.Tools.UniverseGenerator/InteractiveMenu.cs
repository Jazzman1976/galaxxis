namespace Galaxxis.Tools.UniverseGenerator;

/// <summary>
/// A simple text menu for using the tool without remembering the commands. Every menu entry runs the same
/// code as the corresponding command.
/// </summary>
internal static class InteractiveMenu
{
    /// <summary>Shows the menu until the user chooses to exit.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <returns>The process exit code (always 0).</returns>
    public static async Task<int> RunAsync(IServiceProvider services)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Galaxxis universe generator");
            Console.WriteLine("  [1] Generate a universe");
            Console.WriteLine("  [2] List universes");
            Console.WriteLine("  [3] Resume a universe");
            Console.WriteLine("  [4] Pause a universe");
            Console.WriteLine("  [5] Create the admin account");
            Console.WriteLine("  [0] Exit");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine()?.Trim();
            if (choice is null or "0")
            {
                return 0;
            }

            // Reuse the command implementations by building the equivalent command line.
            string[]? command = choice switch
            {
                "1" => ["generate", .. AskGenerateOptions()],
                "2" => ["list"],
                "3" => ["resume", Ask("Universe id")],
                "4" => ["pause", Ask("Universe id")],
                "5" => ["bootstrap-admin"],
                _ => null,
            };

            if (command is null)
            {
                Console.WriteLine("Please choose one of the numbers shown.");
                continue;
            }

            await GeneratorCommands.RunAsync(services, command);
        }
    }

    private static IEnumerable<string> AskGenerateOptions()
    {
        Console.WriteLine("Press Enter to keep a default.");

        foreach ((string option, string question) in new[]
        {
            ("name", "Name"),
            ("seed", "Seed"),
            ("galaxies", "Galaxies"),
            ("systems", "Star systems per galaxy"),
            ("planets", "Planets per star system"),
            ("tick-length", "Tick length (hh:mm:ss)"),
        })
        {
            string answer = Ask(question);
            if (answer.Length > 0)
            {
                yield return $"--{option}";
                yield return answer;
            }
        }
    }

    private static string Ask(string question)
    {
        Console.Write($"{question}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }
}
