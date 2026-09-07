using Content.Goobstation.Shared.Xenobiology.Components.Equipment;
using Content.Shared.Destructible;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

public abstract partial class SharedXenoVacuumSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    private EntityQuery<MobStateComponent> _mobQuery = default!;
    private EntityQuery<UseDelayComponent> _delayQuery = default!;
    private EntityQuery<XenoVacuumTankComponent> _tankQuery = default!;

    private const string ReleaseDelayId = "release";
    private const string SuctionDelayId = "suction";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoVacuumTankComponent, ComponentInit>(OnTankInit);
        SubscribeLocalEvent<XenoVacuumTankComponent, ExaminedEvent>(OnTankExamined);
        SubscribeLocalEvent<XenoVacuumTankComponent, DestructionEventArgs>(OnTankDestruction);
        SubscribeLocalEvent<XenoVacuumComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<XenoVacuumComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<XenoVacuumComponent, AfterInteractEvent>(OnAfterInteract);

        _mobQuery = GetEntityQuery<MobStateComponent>();
        _delayQuery = GetEntityQuery<UseDelayComponent>();
        _tankQuery = GetEntityQuery<XenoVacuumTankComponent>();
    }

    private void OnTankInit(Entity<XenoVacuumTankComponent> ent, ref ComponentInit args)
    {
        ent.Comp.StorageTank = _container.EnsureContainer<Container>(ent, ent.Comp.TankContainerName);
    }

    private void OnTankExamined(Entity<XenoVacuumTankComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var text = Loc.GetString("xeno-vacuum-examined", ("n", ent.Comp.StorageTank.ContainedEntities.Count));
        args.PushMarkup(text);
    }

    private void OnTankDestruction(Entity<XenoVacuumTankComponent> ent, ref DestructionEventArgs args)
    {
        // apparently ContainerManager doesn't automatically release them so
        _container.EmptyContainer(ent.Comp.StorageTank);
    }

    private void OnGotEquippedHand(Entity<XenoVacuumComponent> ent, ref GotEquippedHandEvent args)
    {
        SetTankNozzle(args.User, ent);
        if (GetTank(args.User) is not { } tank)
            return;

        tank.Comp.LinkedNozzle = ent;
        Dirty(tank);
    }

    private void OnGotUnequippedHand(Entity<XenoVacuumComponent> ent, ref GotUnequippedHandEvent args)
    {
        SetTankNozzle(args.User, null);
    }

    private void OnAfterInteract(Entity<XenoVacuumComponent> ent, ref AfterInteractEvent args)
    {
        var delay = _delayQuery.Comp(ent);
        if (CheckDelays((ent, delay))) return;

        if (args.CanReach && args.Target is { } target && _mobQuery.HasComp(target))
        {
            TryDoSuction(args.User, target, ent);
            _useDelay.TryResetDelay((ent, delay), false, SuctionDelayId);
            return;
        }

        if (GetTank(args.User) is not { } tank || tank.Comp.StorageTank.ContainedEntities.Count <= 0)
            return;

        foreach (var removedEnt in _container.EmptyContainer(tank.Comp.StorageTank))
        {
            var identity = Identity.Entity(removedEnt, EntityManager);
            var popup = Loc.GetString("xeno-vacuum-clear-popup", ("ent", identity));
            _popup.PopupPredicted(popup, ent, args.User);

            var coords = args.Target is { } targett ? Transform(targett).Coordinates : args.ClickLocation;
            _throw.TryThrow(removedEnt, coords);

            _stun.TryUpdateParalyzeDuration(removedEnt, TimeSpan.FromSeconds(2));
            SetHTNEnabled(removedEnt, true, 2f);
        }

        _useDelay.TryResetDelay((ent, delay), false, ReleaseDelayId);

        _audio.PlayPredicted(ent.Comp.ClearSound, ent, args.User, AudioParams.Default.WithVolume(-2f));
    }

    #region Helpers
    private bool CheckDelays(Entity<UseDelayComponent?> ent)
        => _useDelay.IsDelayed(ent, SuctionDelayId)
        || _useDelay.IsDelayed(ent, ReleaseDelayId);

    private Entity<XenoVacuumTankComponent>? GetTank(EntityUid user)
    {
        foreach (var item in _hands.EnumerateHeld(user))
        {
            if (_tankQuery.TryComp(item, out var comp))
                return (item, comp);
        }

        if (!_inventory.TryGetContainerSlotEnumerator(user, out var slotEnum, SlotFlags.WITHOUT_POCKET))
            return null;

        while (slotEnum.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } item)
                continue;

            if (_tankQuery.TryComp(item, out var comp))
                return (item, comp);
        }

        return null;
    }

    private void SetTankNozzle(EntityUid user, EntityUid? nozzle)
    {
        if (GetTank(user) is not { } tank)
            return;

        tank.Comp.LinkedNozzle = nozzle;
        Dirty(tank);
    }

    private bool TryDoSuction(EntityUid user, EntityUid target, Entity<XenoVacuumComponent> vacuum)
    {
        if (GetTank(user) is not { } tank)
        {
            var noTankPopup = Loc.GetString("xeno-vacuum-suction-fail-no-tank-popup");
            _popup.PopupPredicted(noTankPopup, vacuum, user);
            return false;
        }

        var identity = Identity.Entity(target, EntityManager);
        if (_whitelist.IsWhitelistFail(vacuum.Comp.EntityWhitelist, target))
        {
            var invalidEntityPopup = Loc.GetString("xeno-vacuum-suction-fail-invalid-entity-popup", ("ent", identity));
            _popup.PopupPredicted(invalidEntityPopup, vacuum, user);

            return false;
        }

        if (tank.Comp.StorageTank.ContainedEntities.Count > tank.Comp.MaxEntities)
        {
            var tankFullPopup = Loc.GetString("xeno-vacuum-suction-fail-tank-full-popup");
            _popup.PopupPredicted(tankFullPopup, vacuum, user);

            return false;
        }

        SetHTNEnabled(target, false);

        if (!_container.Insert(target, tank.Comp.StorageTank))
        {
            Log.Error($"{ToPrettyString(user)} failed to insert {ToPrettyString(target)} into {ToPrettyString(tank)}");
            return false;
        }

        _audio.PlayPredicted(vacuum.Comp.Sound, user, user);
        var successPopup = Loc.GetString("xeno-vacuum-suction-succeed-popup", ("ent", identity));
        _popup.PopupPredicted(successPopup, vacuum, user);

        return true;
    }

    protected virtual void SetHTNEnabled(EntityUid uid, bool enabled, float planCooldown = 0f)
    {
    }
    #endregion

}
