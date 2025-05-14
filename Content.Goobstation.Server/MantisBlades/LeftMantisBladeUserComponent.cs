using Robust.Shared.Audio;

namespace Content.Goobstation.Server.MantisBlades;

[RegisterComponent]
public sealed partial class LeftMantisBladeUserComponent : Component, IMantisBladeUserComponent
{
    [DataField]
    public string ActionProto = "ActionToggleLeftMantisBlade";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionUid;

    [DataField]
    public SoundSpecifier? ExtendSound { get; set; } = new SoundPathSpecifier("/Audio/Items/unsheath.ogg"); // TODO: better sounds

    [DataField]
    public SoundSpecifier? RetractSound { get; set; } = new SoundPathSpecifier("/Audio/Items/sheath.ogg");

    [DataField]
    public string BladeProto { get; set; } = "MantisBlade";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? BladeUid { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public bool DisabledByEmp { get; set; }
}
