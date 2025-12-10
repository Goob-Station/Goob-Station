using Content.Shared.Physics;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.PhaseShift;

[RegisterComponent]
public sealed partial class PhaseShiftedComponent : Component
{
    [DataField]
    public ProtoId<StatusEffectPrototype> StatusEffectId = "PhaseShifted";

    [DataField]
    public float MovementSpeedBuff = 1.5f;

    [DataField]
    public int CollisionMask = (int) CollisionGroup.None;

    [DataField]
    public int CollisionLayer = (int) CollisionGroup.None;

    [DataField]
    public EntProtoId PhaseInEffect = "EffectEmpPulseNoSound";

    [DataField]
    public EntProtoId PhaseOutEffect = "EffectEmpPulseNoSound";

    [DataField]
    public SoundSpecifier PhaseInSound = new SoundPathSpecifier(new ResPath("/Audio/_EinsteinEngines/Shadowling/veilin.ogg"));

    [DataField]
    public SoundSpecifier PhaseOutSound =
        new SoundPathSpecifier(new ResPath("/Audio/_EinsteinEngines/Shadowling/veilout.ogg"));

    public int StoredMask;
    public int StoredLayer;
}
