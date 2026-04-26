using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Components.Vassals;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Light.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Server.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerGoHomeSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerGoHomeEvent>(OnGoHome);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodsuckerGoHomeComponent, BloodsuckerComponent>();
        while (query.MoveNext(out var uid, out var goHome, out var vampire))
        {
            if (!goHome.Active)
                continue;

            goHome.Elapsed += frameTime;

            // Validate coffin still exists
            if (!TryComp(uid, out BloodsuckerClaimedCoffinComponent? claimed)
                || !Exists(claimed.Coffin))
            {
                _popup.PopupEntity(
                    Loc.GetString("bloodsucker-gohome-coffin-destroyed"),
                    uid, uid, PopupType.LargeCaution);
                CancelGoHome(uid, goHome);
                continue;
            }

            // First flicker at 2s
            if (!goHome.FiredFlickerOne && goHome.Elapsed >= 2f)
            {
                goHome.FiredFlickerOne = true;
                FlickerNearbyLights(uid, goHome);
            }

            // Second flicker at 4s
            if (!goHome.FiredFlickerTwo && goHome.Elapsed >= 4f)
            {
                goHome.FiredFlickerTwo = true;
                FlickerNearbyLights(uid, goHome);
            }

            // Teleport at 6s
            if (goHome.Elapsed >= goHome.TeleportDelay)
            {
                TeleportToCoffin(uid, goHome, claimed.Coffin);
                continue;
            }

            // Drain blood per second
            DrainBlood(uid, goHome, frameTime);
        }
    }

    private void OnGoHome(Entity<BloodsuckerComponent> ent, ref BloodsuckerGoHomeEvent args)
    {
        if (!TryComp(ent, out BloodsuckerGoHomeComponent? comp))
            return;

        if (comp.Active)
            return;

        // Must have a claimed coffin
        if (!TryComp(ent.Owner, out BloodsuckerClaimedCoffinComponent? claimed)
            || !Exists(claimed.Coffin))
        {
            _popup.PopupEntity(
                Loc.GetString("bloodsucker-gohome-no-coffin"),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // Check blood cost
        if (!TryComp(ent.Owner, out BloodstreamComponent? bloodstream))
            return;

        var currentBlood = bloodstream.BloodSolution is { } sol
            ? (float) sol.Comp.Solution.Volume
            : 0f;

        if (currentBlood < comp.BloodCost)
        {
            _popup.PopupEntity(
                Loc.GetString("bloodsucker-gohome-no-blood"),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(ent.Owner, bloodstream),
            -FixedPoint2.New(comp.BloodCost));

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity),
                -comp.HumanityCost);

        comp.Active = true;
        comp.Elapsed = 0f;
        comp.FiredFlickerOne = false;
        comp.FiredFlickerTwo = false;
        Dirty(ent.Owner, comp);

        _popup.PopupEntity(
            Loc.GetString("bloodsucker-gohome-start"),
            ent.Owner, ent.Owner, PopupType.Medium);

        args.Handled = true;
    }

    private void TeleportToCoffin(EntityUid uid, BloodsuckerGoHomeComponent comp, EntityUid coffin)
    {
        var coords = Transform(uid).Coordinates;

        var dropItems = IsObservedByMortals(uid);

        DropCuffs(uid);

        if (dropItems)
            DropAllItems(uid);

        SpawnAnimal(comp, coords);
        _audio.PlayPvs(comp.TeleportSound, uid);

        // Move vampire into the coffin's container directly
        _storage.Insert(uid, coffin);

        // Then close it
        var closeEv = new StorageCloseAttemptEvent(uid);
        RaiseLocalEvent(coffin, ref closeEv);
        if (!closeEv.Cancelled)
        {
            var beforeClose = new StorageBeforeCloseEvent(new HashSet<EntityUid> { uid }, new HashSet<EntityUid>());
            RaiseLocalEvent(coffin, ref beforeClose);
            if (TryComp(coffin, out EntityStorageComponent? storage))
            {
                storage.Open = false;
                _audio.PlayPvs(storage.CloseSound, coffin);
                var afterClose = new StorageAfterCloseEvent();
                RaiseLocalEvent(coffin, ref afterClose);
                Dirty(coffin, storage);
            }
        }

        _popup.PopupEntity(
            Loc.GetString("bloodsucker-gohome-arrived"),
            uid, uid, PopupType.Medium);

        CancelGoHome(uid, comp);
    }

    private void FlickerNearbyLights(EntityUid uid, BloodsuckerGoHomeComponent comp)
    {
        _audio.PlayPvs(comp.FlickerSound, uid);

        var lights = new HashSet<Entity<PoweredLightComponent>>();
        _lookup.GetEntitiesInRange<PoweredLightComponent>(
            Transform(uid).Coordinates, comp.FlickerRange, lights);

        foreach (var light in lights)
            _ghost.DoGhostBooEvent(light);
    }

    private bool IsObservedByMortals(EntityUid uid)
    {
        if (!TryComp(uid, out TransformComponent? xform))
            return false;

        foreach (var nearby in _lookup.GetEntitiesInRange<MobStateComponent>(
                     xform.Coordinates, 15f))
        {
            if (nearby.Owner == uid)
                continue;

            if (nearby.Comp.CurrentState == MobState.Dead)
                continue;

            if (HasComp<BloodsuckerComponent>(nearby.Owner))
                continue;

            if (HasComp<BloodsuckerVassalComponent>(nearby.Owner))
                continue;

            // Check they have a session (are a player)
            if (!TryComp(nearby.Owner, out ActorComponent? _))
                continue;

            return true;
        }

        return false;
    }

    private void DropCuffs(EntityUid uid)
    {
        if (!TryComp(uid, out CuffableComponent? cuffable) || cuffable.CuffedHandCount == 0)
            return;

        // Keep uncuffing until all cuffs are removed
        while (cuffable.CuffedHandCount > 0)
        {
            var cuffs = cuffable.LastAddedCuffs;
            if (!Exists(cuffs))
                break;
            _cuffable.Uncuff(uid, uid, cuffs);
        }
    }

    private void DropAllItems(EntityUid uid)
    {
        if (TryComp(uid, out HandsComponent? hands))
        {
            foreach (var hand in hands.Hands.Keys.ToList())
            {
                _hands.TryDrop(uid, hand);
            }
        }

        var slots = _inventory.GetSlotEnumerator(uid);
        while (slots.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is EntityUid)
                _inventory.TryUnequip(uid, uid, slot.ID, force: true);
        }
    }

    private void SpawnAnimal(BloodsuckerGoHomeComponent comp, EntityCoordinates coords)
    {
        if (comp.SpawnMobs.Count == 0)
            return;

        // Weighted pick
        var total = 0;
        foreach (var w in comp.SpawnMobs.Values)
            total += w;

        var roll = _random.Next(total);
        var cumulative = 0;
        foreach (var (proto, weight) in comp.SpawnMobs)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                Spawn(proto, coords);
                return;
            }
        }
    }

    private void DrainBlood(EntityUid uid, BloodsuckerGoHomeComponent comp, float frameTime)
    {
        if (!TryComp(uid, out BloodstreamComponent? bloodstream))
            return;

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(uid, bloodstream),
            -FixedPoint2.New(comp.BloodDrainPerSecond * frameTime));
    }

    private void CancelGoHome(EntityUid uid, BloodsuckerGoHomeComponent comp)
    {
        comp.Active = false;
        comp.Elapsed = 0f;
        comp.FiredFlickerOne = false;
        comp.FiredFlickerTwo = false;
        Dirty(uid, comp);
    }
}
