using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Hands.Components;
using Content.Shared.Heretic;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeCosmos()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticCosmicRune>(OnCosmicRune);
        SubscribeLocalEvent<HereticComponent, EventHereticStarTouch>(OnStarTouch);
        SubscribeLocalEvent<HereticComponent, EventHereticStarBlast>(OnStarBlast);
    }

    private void OnStarBlast(Entity<HereticComponent> ent, ref EventHereticStarBlast args)
    {
        if (args.Coords?.IsValid(EntityManager) is not true)
            return;

        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        ShootProjectileSpell(args.Performer, args.Coords.Value, args.Projectile, args.ProjectileSpeed, args.Entity);
    }

    private void OnStarTouch(Entity<HereticComponent> ent, ref EventHereticStarTouch args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (!TryComp(ent, out HandsComponent? hands) || hands.Hands.Count < 1)
            return;

        args.Handled = true;

        if (_net.IsClient)
            return;

        var hadStarTouch = false;

        foreach (var held in _hands.EnumerateHeld(ent, hands))
        {
            if (!HasComp<StarTouchComponent>(held))
                continue;

            hadStarTouch = true;
            QueueDel(held);
        }

        if (hadStarTouch || !_hands.TryGetEmptyHand(ent, out var emptyHand, hands))
            return;

        var touch = Spawn(args.StarTouch, Transform(ent).Coordinates);

        if (!_hands.TryPickup(ent, touch, emptyHand, animate: false, handsComp: hands))
        {
            QueueDel(touch);
            return;
        }

        EnsureComp<StarTouchComponent>(touch).StarTouchAction = args.Action.Owner;
    }

    private void OnCosmicRune(Entity<HereticComponent> ent, ref EventHereticCosmicRune args)
    {
        if (!TryComp(args.Action, out HereticCosmicRuneActionComponent? runeAction))
            return;

        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        if (_net.IsClient)
            return;

        var firstRuneResolved = Exists(runeAction.FirstRune);
        var secondRuneResolved = Exists(runeAction.SecondRune);

        var coords = Transform(ent).Coordinates.SnapToGrid(EntityManager, _mapMan);
        if (firstRuneResolved && secondRuneResolved)
        {
            EnsureComp<FadingTimedDespawnComponent>(runeAction.FirstRune!.Value).Lifetime = 0f;
            var newRune = Spawn(args.Rune, coords);
            _transform.AttachToGridOrMap(newRune);
            var newRuneComp = EnsureComp<HereticCosmicRuneComponent>(newRune);
            var secondRuneComp = EnsureComp<HereticCosmicRuneComponent>(runeAction.SecondRune!.Value);
            newRuneComp.LinkedRune = runeAction.SecondRune.Value;
            secondRuneComp.LinkedRune = newRune;
            DirtyField(newRune, newRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            DirtyField(runeAction.SecondRune.Value, secondRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            runeAction.FirstRune = runeAction.SecondRune.Value;
            runeAction.SecondRune = newRune;
            return;
        }

        if (!firstRuneResolved)
        {
            var newRune = Spawn(args.Rune, coords);
            _transform.AttachToGridOrMap(newRune);
            runeAction.FirstRune = newRune;

            if (!secondRuneResolved)
                return;

            var newRuneComp = EnsureComp<HereticCosmicRuneComponent>(newRune);
            var secondRuneComp = EnsureComp<HereticCosmicRuneComponent>(runeAction.SecondRune!.Value);
            newRuneComp.LinkedRune = runeAction.SecondRune.Value;
            secondRuneComp.LinkedRune = newRune;
            DirtyField(newRune, newRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            DirtyField(runeAction.SecondRune.Value, secondRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            return;
        }


        if (!secondRuneResolved)
        {
            var newRune = Spawn(args.Rune, coords);
            _transform.AttachToGridOrMap(newRune);
            runeAction.SecondRune = newRune;

            if (!firstRuneResolved)
                return;

            var newRuneComp = EnsureComp<HereticCosmicRuneComponent>(newRune);
            var firstRuneComp = EnsureComp<HereticCosmicRuneComponent>(runeAction.FirstRune!.Value);
            newRuneComp.LinkedRune = runeAction.FirstRune.Value;
            firstRuneComp.LinkedRune = newRune;
            DirtyField(newRune, newRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            DirtyField(runeAction.FirstRune.Value, firstRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
        }
    }
}
