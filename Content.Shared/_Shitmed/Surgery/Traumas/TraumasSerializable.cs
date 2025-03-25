using Content.Shared._Shitmed.Surgery.Traumas.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Surgery.Traumas;

[Serializable, NetSerializable]
public enum TraumaType
{
    BoneDamage,
    OrganDamage,
    VeinsDamage,
    NerveDamage, // pain
    Dismemberment,
}

#region Organs

[Serializable, NetSerializable]
public enum OrganSeverity
{
    Normal,
    Damaged,
    Destroyed, // obliterated
}

[ByRefEvent]
public record struct OrganDamagePointChangedEvent(EntityUid Organ, FixedPoint2 CurrentSeverity, FixedPoint2 SeverityDelta);

[ByRefEvent]
public record struct OrganDamageSeverityChanged(EntityUid Organ, OrganSeverity OldSeverity, OrganSeverity NewSeverity);

#endregion

#region Bones

[Serializable, NetSerializable]
public enum BoneSeverity
{
    Normal,
    Damaged,
    Broken, // Ha-ha.
}

[ByRefEvent]
public record struct BoneSeverityPointChangedEvent(EntityUid Bone, BoneComponent BoneComponent, FixedPoint2 CurrentSeverity, FixedPoint2 SeverityDelta);

[ByRefEvent]
public record struct BoneSeverityChangedEvent(EntityUid Bone, BoneSeverity NewSeverity);

#endregion
