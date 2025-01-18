using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BrigLockerComponent : Component
{
    [DataField,AutoNetworkedField]
    public bool Assigned = false;

    [DataField("denySound")]
    public SoundSpecifier? DenySound;

    [DataField("check")]
    public string Check = string.Empty;
}
