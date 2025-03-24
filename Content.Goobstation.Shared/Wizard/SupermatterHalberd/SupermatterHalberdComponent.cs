using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.SupermatterHalberd;

[RegisterComponent, NetworkedComponent]
public sealed partial class SupermatterHalberdComponent : Component
{
    [DataField]
    public TimeSpan ExecuteDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public SoundSpecifier ExecuteSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/supermatter.ogg");

    [DataField]
    public EntProtoId AshProto = "Ash";

    [DataField]
    public EntProtoId ExecuteEffect = "SupermatterFlashEffect";

    [DataField]
    public EntityWhitelist ObliterateWhitelist;
}
