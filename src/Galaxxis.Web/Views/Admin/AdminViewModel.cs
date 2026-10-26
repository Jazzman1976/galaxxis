using Galaxxis.Application.Universes;

namespace Galaxxis.Web.Views.Admin;

/// <summary>Everything the admin page shows: the universes and the form for generating a new one.</summary>
/// <param name="Universes">All universes, oldest first.</param>
/// <param name="Generate">The "generate universe" form, pre-filled or with the values of a refused attempt.</param>
public sealed record AdminViewModel(IReadOnlyList<UniverseSummary> Universes, GenerateUniverseViewModel Generate);
