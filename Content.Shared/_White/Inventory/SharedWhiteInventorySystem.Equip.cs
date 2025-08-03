using Content.Shared._White.Inventory.Components;
using Content.Shared._White.Jump;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._White.Inventory;

public abstract partial class SharedWhiteInventorySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private void InitializeEquip()
    {
        SubscribeLocalEvent<EquipOnCollideComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<EquipOnMeleeHitComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<EquipOnPickUpComponent, GotEquippedHandEvent>(OnPickedUp);
        SubscribeLocalEvent<EquipOnThrownHitComponent, ThrowDoHitEvent>(OnThrowDoHit, before: new[] {typeof(JumpSystem)});
    }

    private void OnCollideEvent(EntityUid uid, EquipOnCollideComponent component, StartCollideEvent args)
    {
        TryEquip(uid, args.OtherEntity, component);
    }

    private void OnMeleeHit(EntityUid uid, EquipOnMeleeHitComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.FirstOrNull() is not {} target)
            return;

        TryEquip(uid, target, component);
    }

    private void OnPickedUp(EntityUid uid, EquipOnPickUpComponent component, GotEquippedHandEvent args)
    {
        TryEquip(uid, args.User, component);
    }

    private void OnThrowDoHit(EntityUid uid, EquipOnThrownHitComponent component, ThrowDoHitEvent args)
    {
        if (args.Handled)
            return;

        TryEquip(uid, args.Target, component);
        args.Handled = true;
    }

    public bool TryEquip(EntityUid uid, EntityUid target, BaseEquipOnComponent component)
    {
        if (_mobState.IsDead(uid)
            || !_random.Prob(component.EquipProb)
            || _entityWhitelist.IsBlacklistPass(component.Blacklist, target)
            || _inventory.TryGetSlotEntity(target, component.BlockingSlot, out var headItem)
                && TryComp<IngestionBlockerComponent>(headItem, out var ingestionBlocker)
                && ingestionBlocker.Enabled)
            return false;

        if (component.Force)
            _inventory.TryUnequip(target, component.Slot, true);
        _inventory.TryEquip(target, uid, component.Slot, true, true);

        return true;
    }
}
