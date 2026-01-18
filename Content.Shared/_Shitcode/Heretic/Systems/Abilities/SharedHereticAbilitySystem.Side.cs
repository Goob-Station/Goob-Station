using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Heretic;
using Content.Shared.Projectiles;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeSide()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticIceSpear>(OnIceSpear);
    }

    private void OnIceSpear(Entity<HereticComponent> ent, ref EventHereticIceSpear args)
    {
        if (!TryComp(args.Action, out IceSpearActionComponent? spearAction))
            return;

        if (!TryComp(ent, out HandsComponent? hands))
            return;

        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        if (_net.IsClient)
            return;

        if (Exists(spearAction.CreatedSpear))
        {
            var spear = spearAction.CreatedSpear.Value;

            // TODO: When heretic spells are made the way wizard spell works don't handle this action if we can't pick it up.
            // It is handled now because it always speaks invocation no matter what.
            if (_hands.IsHolding((ent.Owner, hands), spear) || !_hands.TryGetEmptyHand((ent, hands), out var hand))
                return;

            if (TryComp(spear, out EmbeddableProjectileComponent? embeddable) && embeddable.EmbeddedIntoUid != null)
                _projectile.EmbedDetach(spear, embeddable);

            _transform.AttachToGridOrMap(spear);
            _transform.SetCoordinates(spear, Transform(ent).Coordinates);
            _hands.TryPickup(ent, spear, hand, false, handsComp: hands);
            return;
        }

        var newSpear = Spawn(spearAction.SpearProto, Transform(ent).Coordinates);
        if (!_hands.TryForcePickupAnyHand(ent, newSpear, false, hands))
        {
            QueueDel(newSpear);
            return;
        }

        spearAction.CreatedSpear = newSpear;
        EnsureComp<IceSpearComponent>(newSpear).ActionId = args.Action;
    }
}
