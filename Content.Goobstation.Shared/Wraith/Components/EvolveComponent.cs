using Content.Shared._White.RadialSelector;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EvolveComponent : Component
{
    /// <summary>
    /// Used in UI. The evolutions that you can choose from.
    /// </summary>
    [DataField(required: true)]
    public List<RadialSelectorEntry> AvailableEvolutions = new();

    /// <summary>
    /// The corpses required from AbsorbCorpse in order to evolve
    /// </summary>
    [DataField]
    public int CorpsesRequired = 3;

    [ViewVariables]
    public EntityUid? ActionEnt;

    [ViewVariables]
    public EntProtoId ActionId = "ActionEvolve";
}

/// <summary>
/// Raised when attempting to evolve.
/// </summary>
[ByRefEvent]
public record struct WraithEvolveAttemptEvent(int CorpsesRequired, bool Cancelled = false);
