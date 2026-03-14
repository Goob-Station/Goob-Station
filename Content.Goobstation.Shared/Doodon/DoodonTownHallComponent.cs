using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Doodons;

/// <summary>
/// Central authority for a Doodon village.
/// Tracks all buildings and doodons within its influence.
/// </summary>

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class DoodonTownHallComponent : Component
{
    /// <summary>
    /// Maximum distance (in tiles) that buildings can function.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InfluenceRadius = 7f;

    /// <summary>
    /// All registered doodon buildings.
    /// </summary>
    public HashSet<EntityUid> Buildings = new();

    /// <summary>
    /// All registered doodons.
    /// </summary>
    public HashSet<EntityUid> Doodons = new();

    /// <summary>
    /// Number of dwellings (population cap).
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int DwellingCount => Buildings.Count;

    /// <summary>
    /// Current population.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int DoodonCount => Doodons.Count;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool CanSpawnMoreDoodons => Doodons.Count < Buildings.Count;
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ShowInfluence;
}
