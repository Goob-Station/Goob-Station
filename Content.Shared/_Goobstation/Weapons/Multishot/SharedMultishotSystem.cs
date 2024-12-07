using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._Goobstation.Weapons.Multishot;

public sealed partial class SharedMultishotSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultishotComponent, GotEquippedHandEvent>(OnEquipWeapon);
        SubscribeLocalEvent<MultishotComponent, GotUnequippedHandEvent>(OnUnequipWeapon);
        SubscribeLocalEvent<MultishotComponent, GunRefreshModifiersEvent>(OnRefreshModifiers);
        SubscribeLocalEvent<MultishotComponent, GunShotEvent>(OnShotAttempt);
    }

    private void OnShotAttempt(Entity<MultishotComponent> multishotWeapon, ref GunShotEvent args)
    {
        var comp = multishotWeapon.Comp;

        if (comp.RelatedWeapon == null)
            return;

        // Need to prevent recursive shoot attempting
        if (_handsSystem.GetActiveItem(args.User) != multishotWeapon)
            return;

        if (!TryComp<GunComponent>(comp.RelatedWeapon, out var relatedGun) || !TryComp<GunComponent>(multishotWeapon, out var gun))
            return;

        if (gun.ShootCoordinates == null)
            return;

        if (TryComp(comp.RelatedWeapon.Value, out GunComponent? otherGun))
            otherGun.Target = gun.Target;

        _gunSystem.AttemptShoot(args.User, comp.RelatedWeapon.Value, relatedGun, gun.ShootCoordinates.Value);

        // Synchronizing reload timer
        var reloadDelta = relatedGun.LastFire - gun.LastFire;
        relatedGun.NextFire -= reloadDelta.Duration();
    }

    private void OnRefreshModifiers(Entity<MultishotComponent> multishotWeapon, ref GunRefreshModifiersEvent args)
    {
        var comp = multishotWeapon.Comp;

        if (comp.RelatedWeapon == null)
            return;

        args.MaxAngle *= comp.SpreadMultiplier;
        args.MinAngle *= comp.SpreadMultiplier;
        args.FireRate /= comp.SpreadMultiplier;
    }

    private void OnEquipWeapon(Entity<MultishotComponent> multishotWeapon, ref GotEquippedHandEvent args)
    {
        var comp = multishotWeapon.Comp;

        if (!TryComp<HandsComponent>(args.User, out var handsComp))
            return;

        if (handsComp.Count != 2)
            return;

        // Find first suitable weapon
        foreach (var held in _handsSystem.EnumerateHeld(args.User, handsComp))
        {
            if (held == multishotWeapon.Owner)
                continue;

            if (!TryComp<MultishotComponent>(held, out var multishotHeld))
                continue;

            comp.RelatedWeapon = held;
            multishotHeld.RelatedWeapon = multishotWeapon;

            Dirty(held, multishotHeld);
            Dirty(multishotWeapon, comp);

            _gunSystem.RefreshModifiers(multishotWeapon.Owner);
            _gunSystem.RefreshModifiers(held);

            break;
        }
    }

    private void OnUnequipWeapon(Entity<MultishotComponent> multishotWeapon, ref GotUnequippedHandEvent args)
    {
        var comp = multishotWeapon.Comp;

        if (comp.RelatedWeapon == null)
            return;

        if (!TryComp<MultishotComponent>(comp.RelatedWeapon.Value, out var relatedMultishot))
        {
            comp.RelatedWeapon = null;
            return;
        }

        var tempRelated = comp.RelatedWeapon.Value;

        relatedMultishot.RelatedWeapon = null;
        comp.RelatedWeapon = null;

        _gunSystem.RefreshModifiers(tempRelated);
        _gunSystem.RefreshModifiers(multishotWeapon.Owner);

        Dirty(tempRelated, relatedMultishot);
        Dirty(multishotWeapon, comp);
    }
}
