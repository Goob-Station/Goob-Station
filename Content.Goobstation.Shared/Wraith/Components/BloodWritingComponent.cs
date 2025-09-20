using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BloodWritingComponent : Component
{
    /// <summary>
    ///  The crayon the user is holding
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? BloodCrayon;

    [ViewVariables]
    public string HandName = "bloodCrayon";

    [ViewVariables]
    public EntProtoId BloodCrayonEntId = "CrayonBlood";
}
