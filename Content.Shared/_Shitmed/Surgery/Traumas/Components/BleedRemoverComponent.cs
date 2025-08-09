using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BleedRemoverComponent : Component
{
    /// <summary>
    ///     The severity it requires for the wound infliction to activate, so that space wont be activating this shit.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 SeverityThreshold = FixedPoint2.New(1);

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public FixedPoint2 BleedingRemovalMultiplier = 0.30;
}
