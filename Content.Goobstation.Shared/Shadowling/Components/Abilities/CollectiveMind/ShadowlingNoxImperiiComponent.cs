using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for the Nox Imperii ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingNoxImperiiComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionNoxImperii";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// The seconds it takes for the ability to activate.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(15);
}
