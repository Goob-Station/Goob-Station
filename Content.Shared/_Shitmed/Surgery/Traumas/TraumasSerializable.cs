using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Organ;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas;

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
    Normal = 0,
    Damaged = 1,
    Destroyed = 2, // obliterated
}

[ByRefEvent]
public record struct OrganIntegrityChangedEvent(FixedPoint2 OldIntegrity, FixedPoint2 NewIntegrity);

[ByRefEvent]
public record struct OrganDamageSeverityChanged(OrganSeverity OldSeverity, OrganSeverity NewSeverity);

[ByRefEvent]
public record struct OrganIntegrityChangedEventOnWoundable(Entity<OrganComponent> Organ, FixedPoint2 OldIntegrity, FixedPoint2 NewIntegrity);

[ByRefEvent]
public record struct OrganDamageSeverityChangedOnWoundable(Entity<OrganComponent> Organ, OrganSeverity OldSeverity, OrganSeverity NewSeverity);
[ByRefEvent]
public record struct TraumaChanceDeductionEvent(FixedPoint2 TraumaSeverity, TraumaType TraumaType, FixedPoint2 ChanceDeduction);

[ByRefEvent]
public record struct BeforeTraumaInducedEvent(FixedPoint2 TraumaSeverity, EntityUid TraumaTarget, TraumaType TraumaType, bool Cancelled = false);

[ByRefEvent]
public record struct TraumaInducedEvent(Entity<TraumaComponent> Trauma, EntityUid TraumaTarget, FixedPoint2 TraumaSeverity, TraumaType TraumaType);

[ByRefEvent]
public record struct TraumaBeingRemovedEvent(Entity<TraumaComponent> Trauma, EntityUid TraumaTarget, FixedPoint2 TraumaSeverity, TraumaType TraumaType);

#endregion

#region Bones

[Serializable, NetSerializable]
public enum BoneSeverity
{
    Normal = 0,
    Damaged = 1,
    Cracked = 2,
    Broken = 3, // Ha-ha.
}

[ByRefEvent]
public record struct BoneIntegrityChangedEvent(Entity<BoneComponent> Bone, FixedPoint2 OldIntegrity, FixedPoint2 NewIntegrity);

[ByRefEvent]
public record struct BoneSeverityChangedEvent(Entity<BoneComponent> Bone, BoneSeverity OldSeverity, BoneSeverity NewSeverity);

#endregion
