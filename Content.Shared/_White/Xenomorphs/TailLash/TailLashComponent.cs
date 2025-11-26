using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.TailLash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TailLashComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId TailAnimationId = "WeaponArcThrust";

    [DataField, AutoNetworkedField]
    public FixedPoint2 TailRange = 2;

    [DataField, AutoNetworkedField]
    public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg", AudioParams.Default.WithVolume(-3));

    [DataField]
    public DamageSpecifier TailDamage = new();

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2>? Inject;

    [DataField]
    public EntProtoId? TailLashActionId = "ActionTailLash";

    [ViewVariables]
    public EntityUid? TailLashAction;
}
