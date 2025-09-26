// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.VentCrawling;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.VentCrawler.Tube.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Goobstation.Shared.FloorGoblin;

public sealed partial class StealShoesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly ITileDefinitionManager _tileManager = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedCrawlUnderFloorSystem _crawlUnderFloorSystem = default!;
    [Dependency] private readonly MobStateSystem _mobstate = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StealShoesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StealShoesComponent, StealShoesEvent>(OnStealShoes);
        SubscribeLocalEvent<StealShoesComponent, StealShoesDoAfterEvent>(OnStealShoesDoAfter);
        SubscribeLocalEvent<StealShoesComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<StealShoesComponent, DoAfterAttemptEvent<StealShoesDoAfterEvent>>(OnStealShoesAttempt);
    }

    private void OnMapInit(EntityUid uid, StealShoesComponent component, MapInitEvent args)
    {
        if (component.StealAction == null)
            _actions.AddAction(uid, ref component.StealAction, component.ActionProto);

        _containers.EnsureContainer<Container>(uid, component.ContainerId);
    }

    private void OnStealShoes(EntityUid uid, StealShoesComponent component, StealShoesEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<VentCrawlerComponent>(uid, out var vent) && vent.InTube)
        {
            _popup.PopupPredicted(Loc.GetString("steal-shoes-covered"), uid, uid);
            args.Handled = true;
            return;
        }

        var target = args.Target;

        if (!_interaction.InRangeUnobstructed(uid, target))
            return;

        if (!CanStealHere(uid))
        {
            _popup.PopupPredicted(Loc.GetString("steal-shoes-covered"), uid, uid);
            args.Handled = true;
            return;
        }

        if (!_inventory.TryGetSlotEntity(target, "shoes", out var shoesUid) || shoesUid == null)
        {
            _popup.PopupPredicted(Loc.GetString("steal-shoes-no-shoes"), uid, uid);
            args.Handled = true;
            return;
        }

        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(2), new StealShoesDoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnWeightlessMove = true,
            NeedHand = false,
            AttemptFrequency = AttemptFrequency.EveryTick,
            DuplicateCondition = DuplicateConditions.SameEvent
        };

        if (_doAfter.TryStartDoAfter(dargs))
            args.Handled = true;
    }


    private void OnStealShoesDoAfter(EntityUid uid, StealShoesComponent component, ref StealShoesDoAfterEvent ev)
    {
        if (ev.Handled || ev.Cancelled || ev.Args.Target is not { } target)
            return;

        if (!CanStealHere(uid) ||
            !_interaction.InRangeUnobstructed(uid, target) ||
            !_inventory.TryGetSlotEntity(target, "shoes", out var shoesUid) ||
            shoesUid is not { } shoes ||
            !TryRemoveShoes(target, shoes))
            return;

        var container = _containers.EnsureContainer<Container>(uid, component.ContainerId);
        _containers.Insert(shoes, container);

        if (component.ChompSound is { } chomp)
            _audio.PlayPredicted(chomp, uid, uid);

        _popup.PopupClient(Loc.GetString("steal-shoes-event", ("target", Identity.Name(target, EntityManager)), ("shoes", Name(shoes))), uid, uid);
        _popup.PopupEntity(Loc.GetString("shoes-stolen-target-event"), target, target);

        ev.Handled = true;
    }

    private void OnStealShoesAttempt(EntityUid uid, StealShoesComponent component, ref DoAfterAttemptEvent<StealShoesDoAfterEvent> ev)
    {
        if (ev.Cancelled)
            return;

        if (!CanStealHere(uid))
            ev.Cancel();
    }


    private bool TryRemoveShoes(EntityUid target, EntityUid shoes)
    {
        const string slotId = "shoes";

        if (!_mobstate.IsDead(target))
            return _inventory.TryUnequip(target, slotId, silent: true, predicted: true, reparent: false);

        return _inventory.TryGetSlotContainer(target, slotId, out var slot, out SlotDefinition? _)
            && _containers.Remove(shoes, slot, force: true, reparent: false);
    }


    private void OnMobStateChanged(EntityUid uid, StealShoesComponent component, MobStateChangedEvent args)
    {
        if (!_net.IsServer)
            return;

        if (args.NewMobState != MobState.Dead)
            return;

        var container = _containers.EnsureContainer<Container>(uid, component.ContainerId);

        var dropCoords = Transform(uid).Coordinates;
        var toDrop = new List<EntityUid>(container.ContainedEntities);
        foreach (var ent in toDrop)
        {
            _containers.Remove(ent, container);
            _transform.SetCoordinates(ent, dropCoords);
            var angle = _random.NextFloat(0f, MathF.Tau);
            var speed = _random.NextFloat(2.5f, 4.5f);
            var vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
            if (TryComp<PhysicsComponent>(ent, out var phys))
                _physics.SetLinearVelocity(ent, vel);
        }


        _body.GibBody(uid);
    }

    private bool CanStealHere(EntityUid uid)
    {
        if (TryComp<VentCrawlerComponent>(uid, out var vent) && vent.InTube)
            return false;

        if (!TryComp<CrawlUnderFloorComponent>(uid, out var crawl) || !crawl.Enabled)
            return true;

        return _crawlUnderFloorSystem.IsOnSubfloor(uid);
    }


}
