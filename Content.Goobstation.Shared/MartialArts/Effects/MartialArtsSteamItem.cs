using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Effects;

public sealed partial class MartialArtsStealItemEntityEffectSystem : EntityEffectSystem<HandsComponent, MartialArtsStealItem>
{
    [Dependency] private SharedHandsSystem _hands = default!;

    protected override void Effect(Entity<HandsComponent> target, ref EntityEffectEvent<MartialArtsStealItem> args)
    {
        // Hacky to make this work
        if (!TryComp(target, out MartialArtsComboPerformedComponent? comp))
            throw new InvalidOperationException("No MartialArtsComboPerformedOnComponent on target!");

        var performer = target;
        var performedOn = comp.PerformedOn;

        if (!_hands.TryGetActiveItem(performedOn, out var activeItem)
            || !_hands.TryDrop(performedOn, activeItem.Value)
            || !_hands.TryGetEmptyHand(performer.AsNullable(), out var emptyHand)
            || !_hands.TryPickup(performer, activeItem.Value, emptyHand))
            return;

        _hands.SetActiveHand(performer.AsNullable(), emptyHand);
    }
}

[UsedImplicitly]
public sealed partial class MartialArtsStealItem : EntityEffectBase<MartialArtsStealItem>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Placeholder";
}
