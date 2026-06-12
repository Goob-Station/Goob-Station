using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;

[Serializable, NetSerializable, DataDefinition]
[Prototype("combatAnimation")]
public sealed partial class CombatAnimationPrototype : IPrototype, ICloneable
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public string AnimationType { get; private set; } = "slash";
    [DataField] public float AnimationDuration = 0.15f;

    [DataField] public float FadeoutDuration = 0.05f;

    [DataField] public float FadeoutStartTime = 0.065f;

    [DataField] public float AngleStart = 0f;

    [DataField] public float AngleEnd = 60f;

    [DataField] public bool UseFadeout = true;

    [DataField] public bool TrackUser = true;

    public object Clone()
    {
        return new CombatAnimationPrototype()
        {
            ID = ID,
            AnimationType = AnimationType,
            AnimationDuration = AnimationDuration,
            FadeoutDuration = FadeoutDuration,
            FadeoutStartTime = FadeoutStartTime,
            AngleStart = AngleStart,
            AngleEnd = AngleEnd,
            UseFadeout = UseFadeout,
            TrackUser = TrackUser
        };
    }
}
