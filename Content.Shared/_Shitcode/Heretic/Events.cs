using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic;

[ByRefEvent]
public record struct GetBodyOrganOverrideEvent<T>(Entity<T, OrganComponent>? Organ) where T : IComponent;

[ByRefEvent]
public readonly record struct ConsumingFoodEvent(EntityUid Food, FixedPoint2 Volume);

[ByRefEvent]
public record struct ImmuneToPoisonDamageEvent(bool Immune = false);

[ByRefEvent]
public record struct ExcludeMetabolismGroupsEvent(EntityUid Metabolizer, List<ProtoId<MetabolismGroupPrototype>>? Groups = null);
