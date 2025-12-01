using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Possesion system for slasher although it's actually pretty generic. Basically the same thing as devils possession but it has actions / combat mode.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherPossessionComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEnt;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherPossession";

    /// <summary>
    /// How long you want the possesion to last
    /// </summar>
    [DataField]
    public TimeSpan PossessionDuration = TimeSpan.FromSeconds(45);

    /// <summary>
    /// Mindshield block. basically just exists to show the loc
    /// </summary>
    [DataField]
    public bool DoesMindshieldBlock = true;
}
