using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Components;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Flammability;

public sealed partial class IgniteOnMeleeHitToggledSystem : EntitySystem
{
    [Dependency] private FlammableSystem _flammable = default!;
    [Dependency] private ItemToggleSystem _itemToggle = default!;

    [SubscribeLocalEvent]
    private void OnMeleeHit(EntityUid uid, IgniteOnMeleeHitToggledComponent component, MeleeHitEvent args)
    {
        if (!_itemToggle.IsActivated(uid))
            return;

        foreach (var entity in args.HitEntities)
        {
            if (!TryComp<FlammableComponent>(entity, out var flammable))
                continue;

            _flammable.AdjustFireStacks(entity, component.FireStacks, flammable);

            if (component.FireStacks >= 0)
                _flammable.Ignite(entity, args.Weapon, flammable, args.User);
        }
    }
}
