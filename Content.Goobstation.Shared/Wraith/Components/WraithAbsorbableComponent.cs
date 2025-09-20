using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WraithAbsorbableComponent : Component
{
    /// <summary>
    ///  Whether the user has been absorbed
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Absorbed;
}
