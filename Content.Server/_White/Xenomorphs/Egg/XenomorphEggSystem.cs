using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Server.Popups;
using Content.Shared._White.Xenomorphs.Egg;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Server._White.Xenomorphs.Egg;

public sealed class XenomorphEggSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly FaceHuggerSystem _faceHugger = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphEggComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<XenomorphEggComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<XenomorphEggComponent, AttackedEvent>(OnAttacked);
    }

    private void OnInit(EntityUid uid, XenomorphEggComponent component, ComponentInit args)
    {
        if (component.Status != XenomorphEggStatus.Growning)
            return;

        _appearance.SetData(uid, XenomorphEggKey.Key, XenomorphEggVisualsStatus.Growning);
        component.GrownAt = _timing.CurTime + _random.Next(component.MinGrowthTime, component.MaxGrowthTime);
    }

    private void OnActivateInWorld(EntityUid uid, XenomorphEggComponent component, ActivateInWorldEvent args)
    {
        switch (component.Status)
        {
            case XenomorphEggStatus.Grown:
                SetBursting(uid, component);
                return;
            case XenomorphEggStatus.Burst:
                CleanBurstingEgg(uid, args.User, component);
                return;
        }
    }

    private void OnAttacked(EntityUid uid, XenomorphEggComponent component, AttackedEvent args)
    {
        if (component.Status != XenomorphEggStatus.Burst || !HasComp<XenomorphComponent>(args.User))
            return;

        CleanBurstingEgg(uid, args.User, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<XenomorphEggComponent>();
        while (query.MoveNext(out var uid, out var xenomorphEgg))
        {
            switch (xenomorphEgg.Status)
            {
                case XenomorphEggStatus.Growning when time >= xenomorphEgg.GrownAt:
                    xenomorphEgg.Status = XenomorphEggStatus.Grown;
                    _appearance.SetData(uid, XenomorphEggKey.Key, XenomorphEggVisualsStatus.Grown);
                    return;
                case XenomorphEggStatus.Grown when time >= xenomorphEgg.CheckInRangeAt:
                    xenomorphEgg.CheckInRangeAt = time + xenomorphEgg.CheckInRangeDelay;

                    foreach (var entity in _entityLookup.GetEntitiesInRange<InventoryComponent>(Transform(uid).Coordinates, xenomorphEgg.BurstRange))
                    {
                        if (HasComp<GhostComponent>(entity) || HasComp<XenomorphComponent>(entity))
                            continue;

                        SetBursting(uid, xenomorphEgg);
                        return;
                    }

                    return;
                case XenomorphEggStatus.Bursting when time >= xenomorphEgg.BurstAt:
                    SetBurst(uid, xenomorphEgg);
                    return;
            }
        }
    }

    private void CleanBurstingEgg(EntityUid uid, EntityUid user, XenomorphEggComponent component)
    {
        _popup.PopupEntity(Loc.GetString("xenomorph-egg-clean-butsting-egg"), user, user);
        _audio.PlayEntity(component.CleaningSound, user, user);
        EnsureComp<TimedDespawnComponent>(uid).Lifetime = 0.1f;
    }

    private void SetBurst(EntityUid uid, XenomorphEggComponent component)
    {
        component.Status = XenomorphEggStatus.Burst;
        _appearance.SetData(uid, XenomorphEggKey.Key, XenomorphEggVisualsStatus.Burst);

        var coordinates = Transform(uid).Coordinates;
        var spawned = Spawn(component.FaceHuggerPrototype, coordinates);

        if (!TryComp<FaceHuggerComponent>(spawned, out var equipOn))
            return;

        foreach (var entity in _entityLookup.GetEntitiesInRange<InventoryComponent>(coordinates, component.BurstRange))
        {
            if (_faceHugger.TryEquipFaceHugger(spawned, entity, equipOn))
                return;
        }
    }

    private void SetBursting(EntityUid uid, XenomorphEggComponent component)
    {
        component.Status = XenomorphEggStatus.Bursting;
        component.BurstAt = _timing.CurTime + component.BurstingDelay;
        _appearance.SetData(uid, XenomorphEggKey.Key, XenomorphEggVisualsStatus.Bursting);
    }
}
