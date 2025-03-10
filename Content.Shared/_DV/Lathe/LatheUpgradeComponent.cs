using Robust.Shared.GameStates;

namespace Content.Shared._DV.Lathe;

/// <summary>
/// Any non-null fields get copied onto LatheComponent at MapInit.
/// Gets removed from the entity after its work is done.
/// </summary>
/// <remarks>
/// Only exists because ComponentRegistry / AddComponent bulldozes existing fields unlike prototype composition.
/// </remarks>
[RegisterComponent, NetworkedComponent, Access(typeof(LatheUpgradeSystem))]
public sealed partial class LatheUpgradeComponent : Component
{
    [DataField]
    public float? TimeMultiplier;

    [DataField]
    public float? MaterialUseMultiplier;
}
