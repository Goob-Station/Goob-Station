using System.Numerics;
using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Explosion;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Throwing;
using Content.Shared.Weather;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModLoaderMiningModuleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private ITileDefinitionManager _tileDefs = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private ItemToggleSystem _itemToggle = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedWeatherSystem _weather = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private EntityQuery<MapGridComponent> _gridQuery = default!;
    [Dependency] private EntityQuery<WeatherComponent> _weatherQuery = default!;

    private const float MagnetDropOffset = 1f;
    private const float MagnetPullRange = 1.5f;

    private readonly HashSet<Entity<PhysicsComponent>> _magnetCandidates = new();

    [SubscribeLocalEvent]
    private void OnAshDamageModify(Entity<ModAshCoatComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.Layers <= 0 || ent.Comp.LayerReductions.Count == 0)
            return;

        var modifiers = new DamageModifierSet();
        foreach (var (type, reduction) in ent.Comp.LayerReductions)
        {
            modifiers.Coefficients[type] = MathF.Max(0f, 1f - reduction * ent.Comp.Layers);
        }

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifiers);
    }

    [SubscribeLocalEvent]
    private void OnAshExplosionResistance(Entity<ModAshCoatComponent> ent, ref GetExplosionResistanceEvent args)
    {
        args.DamageCoefficient *= MathF.Max(0f, 1f - ent.Comp.LayerExplosionCoefficient * ent.Comp.Layers);
    }

    [SubscribeLocalEvent]
    private void OnAshRefreshSpeed(Entity<ModAshCoatComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.LifeStage <= ComponentLifeStage.Running && ent.Comp.Layers >= ent.Comp.MaxLayers)
            args.ModifySpeed(ent.Comp.FullSpeedModifier, ent.Comp.FullSpeedModifier);
    }

    [SubscribeLocalEvent]
    private void OnAshShutdown(Entity<ModAshCoatComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent) && ent.Comp.Layers >= ent.Comp.MaxLayers)
            _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void UpdateAsh(float frameTime)
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ModAshCoatComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var coat, out var xform))
        {
            if (xform.GridUid is not { } gridUid
                || xform.ParentUid != gridUid
                || !_gridQuery.TryComp(gridUid, out var grid))
                continue;

            var tile = _map.GetTileRef((gridUid, grid), xform.Coordinates);
            var change = 0;

            if (coat.LastGrid != gridUid || coat.LastTile != tile.GridIndices)
            {
                var first = coat.LastTile == null;
                coat.LastGrid = gridUid;
                coat.LastTile = tile.GridIndices;
                if (!first && !tile.Tile.IsEmpty)
                    change = ClassifyTile(coat, _tileDefs[tile.Tile.TypeId].ID);
            }

            if (change <= 0 && now >= coat.NextWeatherLayer && InAccretionWeather(coat, xform, (gridUid, grid), tile))
            {
                coat.NextWeatherLayer = now + coat.WeatherInterval;
                change = 1;
            }

            if (change != 0)
                SetLayers((uid, coat), coat.Layers + change);
        }
    }

    private int ClassifyTile(ModAshCoatComponent coat, string tileId)
    {
        foreach (var prefix in coat.KeepTilePrefixes)
        {
            if (tileId.StartsWith(prefix, StringComparison.Ordinal))
                return 0;
        }

        foreach (var prefix in coat.AccretionTilePrefixes)
        {
            if (tileId.StartsWith(prefix, StringComparison.Ordinal))
                return 1;
        }

        return -1;
    }

    private bool InAccretionWeather(ModAshCoatComponent coat, TransformComponent xform, Entity<MapGridComponent> grid, TileRef tile)
    {
        if (coat.AccretionWeather.Count == 0 || !_weatherQuery.TryComp(xform.MapUid, out var weather))
            return false;

        var raging = false;
        foreach (var id in coat.AccretionWeather)
        {
            if (weather.Weather.TryGetValue(id, out var data) && data.State == WeatherState.Running)
            {
                raging = true;
                break;
            }
        }

        return raging && _weather.CanWeatherAffect(grid, grid.Comp, tile);
    }

    private void SetLayers(Entity<ModAshCoatComponent> ent, int layers)
    {
        layers = Math.Clamp(layers, 0, ent.Comp.MaxLayers);
        var old = ent.Comp.Layers;
        if (old == layers)
            return;

        ent.Comp.Layers = layers;
        Dirty(ent);

        var max = ent.Comp.MaxLayers;
        if (layers == max || old == max)
            _movementSpeed.RefreshMovementSpeedModifiers(ent);

        if (layers == max)
        {
            _popup.PopupEntity(Loc.GetString("modsuit-ash-full"), ent, ent);
            _audio.PlayPvs(ent.Comp.FullSound, ent);
        }
        else if (layers == 0)
        {
            _popup.PopupEntity(Loc.GetString("modsuit-ash-empty"), ent, ent);
        }
    }

    [SubscribeLocalEvent]
    private void OnMagnetAction(Entity<ModMagnetModuleComponent> ent, ref ModMagnetActionEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;
        var target = _transform.ToMapCoordinates(args.Target);

        if (TryComp<PullerComponent>(user, out var puller)
            && puller.Pulling is { } pulled
            && _whitelist.IsWhitelistPass(ent.Comp.Whitelist, pulled))
        {
            if (!_modSuit.TryStartModuleUse(ent, user))
                return;

            args.Handled = true;
            if (TryComp<PullableComponent>(pulled, out var pullable))
                _pulling.TryStopPull(pulled, pullable, user, true);

            var from = _transform.GetMapCoordinates(pulled).Position;
            var direction = target.Position - from;
            if (direction.Length() > ent.Comp.LaunchRange)
                direction = direction.Normalized() * ent.Comp.LaunchRange;

            _audio.PlayPredicted(ent.Comp.Sound, pulled, user);
            _throwing.TryThrow(pulled, direction, ent.Comp.LaunchSpeed, user, pushbackRatio: 0f, recoil: false, doSpin: false);
            return;
        }

        if (FindMagnetTarget(ent, user, target) is not { } locker)
        {
            _popup.PopupClient(Loc.GetString("modsuit-magnet-invalid"), user, user);
            return;
        }

        if (!_modSuit.TryStartModuleUse(ent, user))
            return;

        args.Handled = true;

        var userPos = _transform.GetMapCoordinates(user).Position;
        var lockerPos = _transform.GetMapCoordinates(locker).Position;
        var toLocker = lockerPos - userPos;
        var stop = toLocker.LengthSquared() > MagnetDropOffset * MagnetDropOffset
            ? userPos + toLocker.Normalized() * MagnetDropOffset
            : lockerPos;

        var magnetized = EnsureComp<ModMagnetizedComponent>(locker);
        magnetized.Puller = user;
        Dirty(locker, magnetized);

        _audio.PlayPredicted(ent.Comp.Sound, locker, user);
        _throwing.TryThrow(locker, stop - lockerPos, ent.Comp.PullSpeed, user, pushbackRatio: 0f, recoil: false, doSpin: false);
    }

    private EntityUid? FindMagnetTarget(Entity<ModMagnetModuleComponent> ent, EntityUid user, MapCoordinates target)
    {
        EntityUid? best = null;
        var bestDistance = float.MaxValue;
        _magnetCandidates.Clear();
        _lookup.GetEntitiesInRange(target, ent.Comp.PickRange, _magnetCandidates, LookupFlags.Dynamic | LookupFlags.Approximate);
        foreach (var candidate in _magnetCandidates)
        {
            if (candidate.Owner == user
                || Transform(candidate).Anchored
                || _container.IsEntityInContainer(candidate)
                || !_whitelist.IsWhitelistPass(ent.Comp.Whitelist, candidate))
                continue;

            var distance = (_transform.GetMapCoordinates(candidate).Position - target.Position).LengthSquared();
            if (distance >= bestDistance)
                continue;

            best = candidate.Owner;
            bestDistance = distance;
        }

        return best;
    }

    [SubscribeLocalEvent]
    private void OnMagnetizedStop(Entity<ModMagnetizedComponent> ent, ref StopThrowEvent args)
    {
        if (ent.Comp.Puller is { } puller
            && !TerminatingOrDeleted(puller)
            && _transform.InRange(ent.Owner, puller, MagnetPullRange))
            _pulling.TryStartPull(puller, ent);

        RemCompDeferred<ModMagnetizedComponent>(ent);
    }

    [SubscribeLocalEvent]
    private void OnCandelaActivated(Entity<ModCandelaModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        ent.Comp.LastBeacon = null;
        ent.Comp.WarnedEmpty = false;
    }

    private void UpdateCandela()
    {
        var query = EntityQueryEnumerator<ModCandelaModuleComponent, ModModuleComponent>();
        while (query.MoveNext(out var uid, out var candela, out var module))
        {
            if (!module.Active
                || !_modSuit.TryGetModuleSuit(uid, out var suit)
                || suit.Comp.Wearer is not { } wearer)
                continue;

            var wearerXform = Transform(wearer);
            if (wearerXform.GridUid is not { } gridUid || wearerXform.ParentUid != gridUid)
                continue;

            var position = _transform.GetMapCoordinates(wearer, wearerXform);
            if (candela.LastBeacon is { } last
                && last.MapId == position.MapId
                && (last.Position - position.Position).Length() < candela.Spacing)
                continue;

            if (FindBeacon(suit, candela) is not { } beacon)
            {
                if (!candela.WarnedEmpty)
                {
                    candela.WarnedEmpty = true;
                    _popup.PopupEntity(Loc.GetString("modsuit-candela-empty"), wearer, wearer);
                }

                continue;
            }

            candela.WarnedEmpty = false;
            candela.LastBeacon = position;

            _container.TryRemoveFromContainer(beacon, true);
            _transform.SetCoordinates(beacon, wearerXform.Coordinates);
            _transform.AnchorEntity(beacon);
            _itemToggle.TryActivate(beacon, showPopup: false);
            _audio.PlayPvs(candela.Sound, beacon);
        }
    }

    private EntityUid? FindBeacon(Entity<ModSuitComponent> suit, ModCandelaModuleComponent candela)
    {
        if (!TryComp<StorageComponent>(suit, out var storage))
            return null;

        foreach (var item in storage.Container.ContainedEntities)
        {
            if (_whitelist.IsWhitelistPass(candela.Whitelist, item))
                return item;
        }

        return null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        UpdateAsh(frameTime);
        UpdateCandela();
    }
}

public sealed partial class ModMagnetActionEvent : WorldTargetActionEvent;
