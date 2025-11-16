using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.Spook;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Server.Actions;
using Content.Server.Doors.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Actions.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Humanoid;

namespace Content.Goobstation.Server.Wraith;

public sealed class SpookActionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;
    [Dependency] private readonly DoorSystem _door = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private EntityQuery<PoweredLightComponent> _poweredLightQuery;
    private EntityQuery<DoorComponent> _doorQuery;
    private EntityQuery<EntityStorageComponent> _entityStorageQuery;
    private EntityQuery<ApcComponent> _apcQuery;
    private EntityQuery<ActionComponent> _actionQuery;
    private EntityQuery<FlammableComponent> _flammable;
    private EntityQuery<HumanoidAppearanceComponent> _humanoidAppearanceQuery;

    public override void Initialize()
    {
        base.Initialize();

        _poweredLightQuery = GetEntityQuery<PoweredLightComponent>();
        _doorQuery = GetEntityQuery<DoorComponent>();
        _entityStorageQuery = GetEntityQuery<EntityStorageComponent>();
        _apcQuery = GetEntityQuery<ApcComponent>();
        _actionQuery = GetEntityQuery<ActionComponent>();
        _flammable = GetEntityQuery<FlammableComponent>();
        _humanoidAppearanceQuery = GetEntityQuery<HumanoidAppearanceComponent>();

        SubscribeLocalEvent<SpookMarkComponent, SpookEvent>(OnSpookEvent);

        SubscribeLocalEvent<FlipLightsComponent, FlipLightsEvent>(OnFlipLights);
        SubscribeLocalEvent<BurnLightsComponent, BurnLightsEvent>(OnBurnLights);
        SubscribeLocalEvent<OpenDoorsSpookComponent, OpenDoorsSpookEvent>(OnOpenDoors);
        SubscribeLocalEvent<CreateSpookSmokeComponent, CreateSmokeSpookEvent>(OnCreateSmoke);
        SubscribeLocalEvent<CreateEctoplasmComponent, CreateEctoplasmEvent>(OnCreateEctoplasm);
        SubscribeLocalEvent<SapAPCComponent, SapApcEvent>(OnSapAPC);
        SubscribeLocalEvent<RandomSpookComponent, RandomSpookEvent>(OnRandomSpook);
    }
    // todo: Haunt PDAs (ngl js do it in part 2 im tired)

    private void OnSpookEvent(Entity<SpookMarkComponent> ent, ref SpookEvent args)
    {
        if (ent.Comp.SpookEntity is {} spook)
            QueueDel(spook);

        var spookEnt = SpawnAtPosition(ent.Comp.Spook, args.Target);
        ent.Comp.SpookEntity = spookEnt;
        _popup.PopupEntity(Loc.GetString("spook-on-create"), ent.Owner, PopupType.Medium);

        args.Handled = true;
    }

    private void OnFlipLights(Entity<FlipLightsComponent> ent, ref FlipLightsEvent args)
    {
        // taken from ghost boo system

        if (args.Handled)
            return;

        var entities = _lookup.GetEntitiesInRange(args.Performer, ent.Comp.FlipLightRadius).ToList();
        _random.Shuffle(entities);

        var booCounter = 0;
        foreach (var entity in entities)
        {
            var ev = new GhostBooEvent();
            RaiseLocalEvent(entity, ev);

            if (ev.Handled)
                booCounter++;

            if (booCounter >= ent.Comp.FlipLightMaxTargets)
                break;
        }

        _popup.PopupEntity(Loc.GetString("spook-flip-lights"), ent.Owner, PopupType.Medium);
        args.Handled = true;
    }

    private void OnBurnLights(Entity<BurnLightsComponent> ent, ref BurnLightsEvent args)
    {
        var entities = _lookup.GetEntitiesInRange(args.Performer, ent.Comp.SearchRadius).ToList();
        _random.Shuffle(entities);

        var lightBrokenCounter = 0;
        foreach (var entity in entities)
        {
            if (lightBrokenCounter > ent.Comp.MaxBurnLights)
                break;

            if (!_poweredLightQuery.TryComp(entity, out var poweredLight))
                continue;

            _poweredLight.TryDestroyBulb(entity, poweredLight);

            var bulbLookup = _lookup.GetEntitiesInRange(entity, ent.Comp.Range);
            foreach (var target in bulbLookup)
            {
                if (!_flammable.TryGetComponent(target, out var fl) || _humanoidAppearanceQuery.HasComp(target))
                    continue;

                fl.FireStacks += ent.Comp.FireStack.Next(_random);
                _flammableSystem.Ignite(target, entity, fl);
            }

            lightBrokenCounter++;
        }

        _popup.PopupEntity(Loc.GetString("spook-burn-lights"), ent.Owner, PopupType.Medium);
        args.Handled = true;
    }

    private void OnOpenDoors(Entity<OpenDoorsSpookComponent> ent, ref OpenDoorsSpookEvent args)
    {
        var entities = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRadius).ToList();
        _random.Shuffle(entities);

        var openedCounter = 0;
        foreach (var entity in entities)
        {
            if (openedCounter > ent.Comp.MaxContainer)
                break;

            if (_entityStorageQuery.HasComp(entity))
            {
                _entityStorage.OpenStorage(entity);
                openedCounter++;
                continue;
            }

            if (_doorQuery.HasComp(entity))
            {
                _door.TryOpen(entity);
                openedCounter++;
            }
        }

        args.Handled = true;
    }

    private void OnCreateSmoke(Entity<CreateSpookSmokeComponent> ent, ref CreateSmokeSpookEvent args)
    {
        // TODO make reagent that makes you drop items in smoke
        var grid = _transform.GetGrid(ent.Owner);
        var center = Transform(ent.Owner).Coordinates;
        var map = _transform.GetMap(ent.Owner);

        if (map == null || grid == null)
            return;

        for (var i = 0; i < ent.Comp.SmokeAmount; i++)
        {
            var offsetX = _random.Next(-ent.Comp.SearchRange, ent.Comp.SearchRange + 1);
            var offsetY = _random.Next(-ent.Comp.SearchRange, ent.Comp.SearchRange + 1);

            var targetCoords = new EntityCoordinates(grid.Value, center.X + offsetX, center.Y + offsetY);

            var smokeEnt = SpawnAtPosition(ent.Comp.SmokeProto, targetCoords);
            _smoke.StartSmoke(smokeEnt, ent.Comp.SmokeSolution, ent.Comp.Duration, ent.Comp.SpreadAmount);
        }

        _popup.PopupEntity(Loc.GetString("spook-create-smoke"), ent.Owner, PopupType.Medium);
        args.Handled = true;
    }

    private void OnCreateEctoplasm(Entity<CreateEctoplasmComponent> ent, ref CreateEctoplasmEvent args)
    {
        var grid = _transform.GetGrid(ent.Owner);
        var center = Transform(ent.Owner).Coordinates;
        var map = _transform.GetMap(ent.Owner);

        if (map == null || grid == null)
            return;

        var amount = _random.Next(ent.Comp.AmountMinMax.X, ent.Comp.AmountMinMax.Y + 1);
        for (var i = 0; i < amount; i++)
        {
            var offsetX = _random.Next(-ent.Comp.SearchRange, ent.Comp.SearchRange + 1);
            var offsetY = _random.Next(-ent.Comp.SearchRange, ent.Comp.SearchRange + 1);

            var targetCoords = new EntityCoordinates(grid.Value, center.X + offsetX, center.Y + offsetY);

            SpawnAtPosition(ent.Comp.EctoplasmProto, targetCoords);
        }

        _popup.PopupEntity(Loc.GetString("spook-create-ectoplasm"), ent.Owner, PopupType.Medium);
        args.Handled = true;
    }

    private void OnSapAPC(Entity<SapAPCComponent> ent, ref SapApcEvent args)
    {
        var chargeToRemove = ent.Comp.ChargeToRemove;

        if (TryComp<PassiveWraithPointsComponent>(ent.Owner, out var passiveWraithPoints))
            chargeToRemove *= (float)passiveWraithPoints.WpGeneration;

        var looup = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange).ToList();
        _random.Shuffle(looup);

        foreach (var entity in looup)
        {
            if (!_apcQuery.HasComp(entity))
                continue;

            _battery.ChangeCharge(entity, -chargeToRemove);
            _popup.PopupEntity(Loc.GetString("spook-apc-sap"), entity, PopupType.MediumCaution);
            break;
        }
        args.Handled = true;
    }

    private void OnRandomSpook(Entity<RandomSpookComponent> ent, ref RandomSpookEvent args)
    {
        var actions = _actions.GetActions(ent.Owner).ToList();
        _random.Shuffle(actions);

        foreach (var action in actions)
        {
            if (!_actionQuery.TryComp(action, out var actionComp)
                || action == args.Action // skip itself
                || _actions.IsCooldownActive(actionComp, _timing.CurTime))
            {
                _popup.PopupEntity(Loc.GetString("spook-on-cooldown"), ent.Owner);
                continue;
            }

            _actions.PerformAction(args.Performer, action);
            _actions.StartUseDelay(action.Owner);
            break;
        }

        args.Handled = true;
    }
}
