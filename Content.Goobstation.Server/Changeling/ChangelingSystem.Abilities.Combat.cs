using System.Linq;
using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.InternalResources.Components;
using Content.Server.Light.Components;
using Content.Shared._Goobstation.Weapons.AmmoSelector;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingSystem
{
    private void SubscribeCombatAbilities()
    {
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleArmbladeEvent>(OnToggleArmblade);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleArmHammerEvent>(OnToggleHammer);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleArmClawEvent>(OnToggleClaw);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleDartGunEvent>(OnToggleDartGun);
        SubscribeLocalEvent<ChangelingIdentityComponent, CreateBoneShardEvent>(OnCreateBoneShard);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleChitinousArmorEvent>(OnToggleArmor);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleOrganicShieldEvent>(OnToggleShield);
        SubscribeLocalEvent<ChangelingIdentityComponent, ShriekDissonantEvent>(OnShriekDissonant);
        SubscribeLocalEvent<ChangelingIdentityComponent, ShriekResonantEvent>(OnShriekResonant);
        SubscribeLocalEvent<ChangelingIdentityComponent, ToggleStrainedMusclesEvent>(OnToggleStrainedMuscles);
    }


    private bool TryToggleChangelingWeapon(
        Entity<ChangelingIdentityComponent> ent,
        EntProtoId weaponProto,
        bool handled)
    {
        if (handled || !TryToggleItem(ent, weaponProto, out _))
            return false;
        PlayMeatySound(ent);
        return true;
    }

    private void OnToggleArmblade(Entity<ChangelingIdentityComponent> ent, ref ToggleArmbladeEvent args)
        => args.Handled |= TryToggleChangelingWeapon(ent, _armbladePrototype, args.Handled);

    private void OnToggleHammer(Entity<ChangelingIdentityComponent> ent, ref ToggleArmHammerEvent args)
        => args.Handled |= TryToggleChangelingWeapon(ent, _armbladePrototype, args.Handled);

    private void OnToggleClaw(Entity<ChangelingIdentityComponent> ent, ref ToggleArmClawEvent args)
        => args.Handled |= TryToggleChangelingWeapon(ent, _armbladePrototype, args.Handled);

    // TODO: COME BACK TO THIS
    private void OnToggleDartGun(Entity<ChangelingIdentityComponent> ent, ref ToggleDartGunEvent args)
    {
        if (args.Handled)
            return;

        var chemCostOverride = GetEquipmentChemCostOverride(ent.Comp, _dartGunPrototype);

        if (!TryToggleItem(ent, _dartGunPrototype, out var dartgun))
            return;

        if (!TryComp(dartgun, out AmmoSelectorComponent? ammoSelector))
        {
            PlayMeatySound(ent);
            return;
        }

        if (!_mind.TryGetMind(ent, out var mindId, out _) || !TryComp(mindId, out ActionsContainerComponent? container))
            return;

        var setProto = false;
        foreach (var ability in container.Container.ContainedEntities)
        {
            if (!TryComp(ability, out ChangelingReagentStingComponent? sting) || sting.DartGunAmmo == null)
                continue;

            ammoSelector.Prototypes.Add(sting.DartGunAmmo.Value);

            if (setProto)
                continue;

            _selectableAmmo.TrySetProto((dartgun.Value, ammoSelector), sting.DartGunAmmo.Value);
            setProto = true;
        }

        if (ammoSelector.Prototypes.Count == 0)
        {
            UpdateChemicals(ent, chemCostOverride ?? Comp<InternalResourcesActionComponent>(args.Action).UseAmount);
            _popup.PopupEntity(Loc.GetString("changeling-dartgun-no-stings"), ent, ent);
            ent.Comp.Equipment.Remove(_dartGunPrototype);
            QueueDel(dartgun.Value);
            return;
        }

        Dirty(dartgun.Value, ammoSelector);

        PlayMeatySound(ent);

        args.Handled = true;
    }

    private void OnCreateBoneShard(Entity<ChangelingIdentityComponent> ent, ref CreateBoneShardEvent args)
    {
        if (args.Handled)
            return;

        var star = Spawn(_boneShardPrototype, Transform(ent).Coordinates);
        _hands.TryPickupAnyHand(ent, star);
        PlayMeatySound(ent);

        args.Handled = true;
    }

    private void OnToggleArmor(EntityUid uid, ChangelingIdentityComponent comp, ref ToggleChitinousArmorEvent args)
    {
        if (args.Handled)
            return;

        float? chemCostOverride = comp.ActiveArmor == null ? null : 0f;

        if (!TryToggleArmor(uid, comp, [(_armorHelmetPrototype, "head"), (_armorPrototype, "outerClothing")]))
        {
            _popup.PopupEntity(Loc.GetString("changeling-equip-armor-fail"), uid, uid);
            UpdateChemicals((uid, comp),
                chemCostOverride ?? Comp<InternalResourcesActionComponent>(args.Action).UseAmount);
            return;
        }

        args.Handled = true;
    }

    private void OnToggleShield(Entity<ChangelingIdentityComponent> ent, ref ToggleOrganicShieldEvent args)
    {
        if (args.Handled || !TryToggleItem(ent, _shieldPrototype, out _))
            return;
        PlayMeatySound(ent);
        args.Handled = true;
    }

    private void OnShriekDissonant(Entity<ChangelingIdentityComponent> ent, ref ShriekDissonantEvent args)
    {
        if (args.Handled)
            return;

        DoScreech(ent);

        var pos = _transform.GetMapCoordinates(ent);
        var power = ent.Comp.ShriekPower;
        _emp.EmpPulse(pos, power, 5000f, power * 2);

        args.Handled = true;
    }

    private void OnShriekResonant(Entity<ChangelingIdentityComponent> ent, ref ShriekResonantEvent args)
    {
        if (args.Handled)
            return;

        DoScreech(ent); // screenshake
        TryScreechStun(ent); // the actual thing

        var power = ent.Comp.ShriekPower;
        var lights = GetEntityQuery<PoweredLightComponent>();
        var lookup = _lookup.GetEntitiesInRange(ent, power);

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var light in lookup)
            if (lights.HasComponent(light))
                _light.TryDestroyBulb(light);

        args.Handled = true;
    }

    private void OnToggleStrainedMuscles(Entity<ChangelingIdentityComponent> ent, ref ToggleStrainedMusclesEvent args)
    {
        if (args.Handled)
            return;
        ToggleStrainedMuscles(ent);
        args.Handled = true;
    }

    private void ToggleStrainedMuscles(Entity<ChangelingIdentityComponent> ent)
    {
        if (!ent.Comp.StrainedMusclesActive)
        {
            _popup.PopupEntity(Loc.GetString("changeling-muscles-start"), ent, ent);
            ent.Comp.StrainedMusclesActive = true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("changeling-muscles-end"), ent, ent);
            ent.Comp.StrainedMusclesActive = false;
        }

        PlayMeatySound(ent);
        _speed.RefreshMovementSpeedModifiers(ent);
    }


    private void OnStingReagent(Entity<ChangelingIdentityComponent> ent, ref StingReagentEvent args)
    {
        if (args.Handled || !TryReagentSting(ent, args))
            return;

        args.Handled = true;
    }

    private void OnStingTransform(Entity<ChangelingIdentityComponent> ent, ref StingTransformEvent args)
    {
        if (args.Handled || !TrySting(ent, args, true))
            return;

        var target = args.Target;
        if (!TryTransform(target, ent.Comp, true, true))
            UpdateChemicals(ent, Comp<InternalResourcesActionComponent>(args.Action).UseAmount);

        args.Handled = true;
    }

    private void OnStingFakeArmblade(Entity<ChangelingIdentityComponent> ent, ref StingFakeArmbladeEvent args)
    {
        if (args.Handled || !TrySting(ent, args))
            return;
        var target = args.Target;
        var fakeArmblade = EntityManager.SpawnEntity(_fakeArmbladePrototype, Transform(target).Coordinates);
        var handsValid = _hands.TryForcePickupAnyHand(target, fakeArmblade);

        if (TryComp<HandsComponent>(target, out var handComp)
            && handsValid)
        {
            var weaponCount = _hands.EnumerateHeld((target, handComp)).Count(HasComp<ChangelingFakeWeaponComponent>);
            handsValid = (weaponCount <= 1);
        }

        if (!handsValid)
        {
            QueueDel(fakeArmblade);
            UpdateChemicals(ent, Comp<InternalResourcesActionComponent>(args.Action).UseAmount);
            _popup.PopupEntity(Loc.GetString("changeling-sting-fail-fakeweapon"), ent, ent);
            return;
        }

        PlayMeatySound(ent, target); // hmm

        args.Handled = true;
    }

    private void OnLayEgg(Entity<ChangelingIdentityComponent> ent, ref StingLayEggsEvent args)
    {
        var target = args.Target;

        if (args.Handled || !CanLayEgg(ent, target))
            return;

        var mind = _mind.GetMind(ent);
        if (mind == null)
            return;
        if (!TryComp<StoreComponent>(ent, out var storeComp))
            return;

        ent.Comp.IsInLastResort = false;
        ent.Comp.IsInLesserForm = true;

        var eggComp = EnsureComp<ChangelingEggComponent>(target);
        eggComp.lingComp = ent.Comp;
        eggComp.lingMind = (EntityUid) mind;
        eggComp.lingStore = _serialization.CreateCopy(storeComp, notNullableOverride: true);
        eggComp.AugmentedEyesightPurchased = HasComp<Shared.Overlays.ThermalVisionComponent>(ent);

        EnsureComp<AbsorbedComponent>(target);
        var dmg = new DamageSpecifier(_proto.Index(_absorbedDamageGroup), 200);
        _damage.TryChangeDamage(target, dmg, false, false, targetPart: TargetBodyPart.All); // Shitmed Change
        _blood.ChangeBloodReagent(target, "FerrochromicAcid");
        _blood.SpillAllSolutions(target);

        PlayMeatySound(ent);
        Body.GibBody(ent);

        args.Handled = true;
    }

    private bool CanLayEgg(Entity<ChangelingIdentityComponent> ent, EntityUid target)
    {
        if (!_mobState.IsDead(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-incapacitated"), ent, ent);
            return false;
        }

        if (HasComp<AbsorbedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-absorbed"), ent, ent);
            return false;
        }

        if (!HasComp<AbsorbableComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-unabsorbable"), ent, ent);
            return false;
        }

        if (CheckFireStatus(ent)) // checks if the target is on fire
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-onfire"), ent, ent);
            return false;
        }

        return true;
    }

}
