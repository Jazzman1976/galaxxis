namespace Galaxxis.Domain.Universes.Generation;

/// <summary>
/// Creates unique, pronounceable names for universes, galaxies, star systems, planets and moons.
/// </summary>
/// <remarks>
/// A name combines a random prefix and a random suffix, for example "Nova" + "rax" = "Novarax".
/// If that combination was already used within the same universe, a number is appended, starting with 2
/// ("Novarax2", "Novarax3", …). The word lists are taken over from the legacy universe creator.
/// One instance is used per generated universe, so names are unique within that universe.
/// </remarks>
internal sealed class NameGenerator
{
    private static readonly string[] Prefixes =
    [
        "Andro", "Zeta", "Nova", "Vega", "Xeno", "Orion", "Cryo", "Altair", "Cygn", "Luma",
        "Astr", "Neb", "Draco", "Hydra", "Lyra", "Pavo", "Sirius", "Taur", "Ursa", "Vulc",
        "Zeph", "Aquila", "Pegas", "Scorp", "Aether", "Nyx", "Solari", "Celest", "Nebu", "Thalos",
        "Elyon", "Quor", "Velar", "Arcyn", "Oryn", "Kael", "Myra", "Icar", "Rho", "Syra",
    ];

    private static readonly string[] Suffixes =
    [
        "nex", "rax", "lia", "thar", "lux", "ion", "vex", "mir", "dax", "oria",
        "aris", "zar", "ek", "tor", "zen", "xar", "yra", "kron", "nara", "sor",
        "lith", "dron", "fex", "gala", "hax", "jora", "kira", "vyr", "thys", "quar",
        "exis", "zun", "reth", "vorn", "myr", "tix", "omir", "tari", "sai", "ael",
    ];

    private readonly Random _random;
    private readonly HashSet<string> _usedNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates a name generator drawing from the given random source.</summary>
    /// <param name="random">The seeded random source of the universe being generated.</param>
    public NameGenerator(Random random)
    {
        _random = random;
    }

    /// <summary>Marks a name as used, for example a name the administrator chose for the universe.</summary>
    /// <param name="name">The name to reserve.</param>
    public void Reserve(string name) => _usedNames.Add(name);

    /// <summary>Returns a name that has not been returned or reserved before.</summary>
    /// <returns>A unique name.</returns>
    public string Next()
    {
        string baseName = Prefixes[_random.Next(Prefixes.Length)] + Suffixes[_random.Next(Suffixes.Length)];

        string name = baseName;
        for (int number = 2; _usedNames.Contains(name); number++)
        {
            name = baseName + number;
        }

        _usedNames.Add(name);
        return name;
    }
}
