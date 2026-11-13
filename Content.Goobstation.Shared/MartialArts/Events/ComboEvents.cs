using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Events;

public sealed partial class AfterComboAttackPerformedEvent(
    Entity<ComboPerformerComponent> performer,
    EntityUid target,
    EntityUid weapon,
    ComboAttackType type)
    : CancellableEntityEventArgs
{
    public Entity<ComboPerformerComponent> Performer { get; } = performer;
    public EntityUid Target { get; } = target;
    public EntityUid Weapon { get; } = weapon;
    public ComboAttackType Type { get; } = type;
}

[ByRefEvent]
public record struct GetAvailableCombosEvent(
    Entity<ComboPerformerComponent> Performer,
    EntityUid Target,
    EntityUid Weapon,
    List<ProtoId<MartialArtsComboPrototype>> Combos
);

[ByRefEvent]
public readonly record struct Performed65Event(EntityUid PerformedOn);
