using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Screech;

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphScreechComponent : Component
{
    [DataField]
    public float Range = 12f;

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(5);

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier(
        "/Audio/_RMC14/Xeno/alien_queen_screech.ogg",
        AudioParams.Default.WithVolume(3f).WithMaxDistance(28f));

    [DataField]
    public EntProtoId ActionId = "ActionXenomorphScreech";

    [DataField]
    public EntProtoId? EffectPrototype = "EffectXenomorphScreech";

    [ViewVariables]
    public EntityUid? ActionEntity;
}

public sealed partial class XenomorphScreechActionEvent : InstantActionEvent;
