// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Components;
using Content.Server.Audio;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Shared.Atmos;
using Content.Shared.Audio;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Damage;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weather;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;
using System.Numerics;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Effects;
using Content.Shared.Heretic;
using Content.Shared.Movement.Components;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Server.Heretic.EntitySystems.PathSpecific;

// void path heretic exclusive
public sealed class AristocratSystem : EntitySystem
{
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _globalSound = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    private static readonly EntProtoId IceTilePrototype = "IceCrust";
    private static readonly ProtoId<ContentTileDefinition> SnowTilePrototype = "FloorAstroSnow";
    private static readonly EntProtoId IceWallPrototype = "WallIce";

    private const float ConduitDelay = 1f;

    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AristocratComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<AristocratComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AristocratComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<AristocratComponent, HitScanReflectAttemptEvent>(OnReflectHitScan);

        SubscribeLocalEvent<VoidAscensionAuraComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<VoidAscensionAuraComponent, EndCollideEvent>(OnEndCollide);

        SubscribeLocalEvent<AffectedByVoidAuraComponent, MoveEvent>(OnMove);
    }

    private void OnMove(Entity<AffectedByVoidAuraComponent> ent, ref MoveEvent args)
    {
        if (ent.Comp.Aura == EntityUid.Invalid || !TryComp(ent.Comp.Aura, out VoidAscensionAuraComponent? aura))
        {
            RemComp(ent, ent.Comp);
            return;
        }

        ProcessAura((ent.Comp.Aura, aura), ent, ent.Comp);
    }

    private void OnEndCollide(Entity<VoidAscensionAuraComponent> ent, ref EndCollideEvent args)
    {
        if (TerminatingOrDeleted(args.OtherEntity))
            return;

        if (!TryComp(args.OtherEntity, out AffectedByVoidAuraComponent? affected) ||
            !TryComp(args.OtherEntity, out PhysicsComponent? physics))
            return;

        if (affected.OldVelocity != null)
        {
            var velocity = physics.LinearVelocity.Normalized() * affected.OldVelocity.Value;
            _physics.SetLinearVelocity(args.OtherEntity, velocity, body: physics);
        }

        RemComp<HomingProjectileComponent>(args.OtherEntity);
        RemComp(args.OtherEntity, affected);
    }

    private void OnStartCollide(Entity<VoidAscensionAuraComponent> ent, ref StartCollideEvent args)
    {
        ProcessAura(ent, args.OtherEntity, physics: args.OtherBody);
    }

    private void ProcessAura(Entity<VoidAscensionAuraComponent> ent,
        EntityUid bullet,
        AffectedByVoidAuraComponent? affected = null,
        PhysicsComponent? physics = null)
    {
        if (!Resolve(bullet, ref physics, false))
            return;

        if (!TryComp(bullet, out ProjectileComponent? projectile))
            return;

        var xform = Transform(ent);
        var parent = xform.ParentUid;
        if (!TryComp(parent, out AristocratComponent? aristocrat))
            return;

        if (projectile.Shooter == parent)
            return;

        affected ??= EnsureComp<AffectedByVoidAuraComponent>(bullet);

        affected.Aura = ent;

        FreezeBullet((parent, aristocrat, null, null), (bullet, projectile, affected, physics));
    }

    private void OnReflectHitScan(Entity<AristocratComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        args.Reflected = true;
    }

    private void OnStartup(Entity<AristocratComponent> ent, ref ComponentStartup args)
    {
        BeginWaltz(ent);
        DoVoidAnnounce(ent, "begin");
    }

    private bool CheckOtherAristocrats(Entity<AristocratComponent> ent)
    {
        var others = EntityQueryEnumerator<AristocratComponent, MobStateComponent>();
        while (others.MoveNext(out var other, out _, out var stateComp))
        {
            if (ent.Owner == other || stateComp.CurrentState == MobState.Dead)
                continue;

            return true;
        }

        return false;
    }

    private void DoVoidAnnounce(Entity<AristocratComponent> ent, string context)
    {
        if (CheckOtherAristocrats(ent))
            return;

        var xform = Transform(ent);

        var victims = EntityQueryEnumerator<ActorComponent, MobStateComponent>();
        while (victims.MoveNext(out var victim, out var actorComp, out var stateComp))
        {
            var xformVictim = Transform(victim);

            if (xformVictim.MapUid != xform.MapUid || stateComp.CurrentState == MobState.Dead ||
                ent.Owner ==
                victim) // DoVoidAnnounce doesn't happen when there's other (alive) ascended void heretics, so you only have to exclude the user
                continue;

            _popup.PopupEntity(Loc.GetString($"void-ascend-{context}"),
                victim,
                actorComp.PlayerSession,
                PopupType.LargeCaution);
        }
    }

    private void BeginWaltz(Entity<AristocratComponent> ent)
    {
        if (CheckOtherAristocrats(ent))
            return;

        _globalSound.DispatchStationEventMusic(ent,
            ent.Comp.VoidsEmbrace,
            StationEventMusicType.VoidAscended,
            AudioParams.Default.WithLoop(true));

        // the fog (snow) is coming
        var xform = Transform(ent);
        _weather.SetWeather(xform.MapID, _prot.Index<WeatherPrototype>("SnowfallMagic"), null);
    }

    private void EndWaltz(Entity<AristocratComponent> ent)
    {
        if (CheckOtherAristocrats(ent))
            return;

        _globalSound.StopStationEventMusic(ent, StationEventMusicType.VoidAscended);

        var xform = Transform(ent);
        _weather.SetWeather(xform.MapID, null, null);
    }

    private void OnMobStateChange(Entity<AristocratComponent> ent, ref MobStateChangedEvent args)
    {
        var stateComp = args.Component;

        if (stateComp.CurrentState == MobState.Dead)
        {
            ent.Comp.HasDied = true;
            EndWaltz(ent); // its over bros
            DoVoidAnnounce(ent, "end");
        }

        // in the rare case that they are revived for whatever reason
        if (stateComp.CurrentState == MobState.Alive && ent.Comp.HasDied)
        {
            ent.Comp.HasDied = false;
            BeginWaltz(ent); // we're back bros
            DoVoidAnnounce(ent, "restart");
        }
    }


    private void OnShutdown(Entity<AristocratComponent> ent, ref ComponentShutdown args)
    {
        EndWaltz(ent); // its over bros
        DoVoidAnnounce(ent, "end");
    }

    private List<EntityCoordinates> GetTiles(EntityCoordinates coords, int range)
    {
        var tiles = new List<EntityCoordinates>();

        for (var y = -range; y <= range; y++)
        {
            for (var x = -range; x <= range; x++)
            {
                var offset = new Vector2(x, y);

                var pos = coords.Offset(offset).SnapToGrid(EntityManager, _mapMan);
                tiles.Add(pos);
            }
        }

        return tiles;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AristocratComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var aristocrat, out var xform))
        {
            aristocrat.UpdateTimer += frameTime;

            if (aristocrat.UpdateTimer < aristocrat.UpdateDelay)
                continue;

            Cycle((uid, aristocrat, xform));
            aristocrat.UpdateTimer = 0;
        }

        _accumulator += frameTime;

        if (_accumulator < ConduitDelay)
            return;

        _accumulator = 0f;

        var airlockQuery = GetEntityQuery<AirlockComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();
        var hereticQuery = GetEntityQuery<HereticComponent>();
        var ghoulQuery = GetEntityQuery<GhoulComponent>();
        var statusQuery = GetEntityQuery<StatusEffectsComponent>();

        HashSet<EntityUid> ignored = new();
        var conduitQuery = EntityQueryEnumerator<VoidConduitComponent, TransformComponent>();
        while (conduitQuery.MoveNext(out var uid, out var conduit, out var xform))
        {
            FreezeAtmos((uid, xform));

            var (pos, rot) = _xform.GetWorldPositionRotation(xform, xformQuery);

            var box = Box2.CenteredAround(pos, Vector2.One * (1f + conduit.Range * 2f));
            var rotated = new Box2Rotated(box, rot, pos);

            List<EntityUid> affected = new();
            var result = _lookup.GetEntitiesIntersecting(xform.MapID, rotated);
            foreach (var ent in result)
            {
                if (ignored.Contains(ent))
                    continue;

                if (hereticQuery.HasComp(ent) || ghoulQuery.HasComp(ent))
                {
                    ignored.Add(ent);
                    if (statusQuery.TryComp(ent, out var status))
                    {
                        _status.TryAddStatusEffect<PressureImmunityComponent>(ent,
                            "PressureImmunity",
                            TimeSpan.FromSeconds(2),
                            true,
                            status);
                    }
                    continue;
                }

                if (_voidcurse.DoCurse(ent))
                {
                    ignored.Add(ent);
                    affected.Add(ent);
                    continue;
                }

                var dmg = conduit.StructureDamage;

                if (airlockQuery.HasComp(ent))
                {
                    _audio.PlayPvs(conduit.AirlockDamageSound, Transform(ent).Coordinates);
                    ignored.Add(ent);
                    affected.Add(ent);
                    _damage.TryChangeDamage(ent,
                        dmg * _rand.NextFloat(conduit.MinMaxAirlockDamageMultiplier.X,
                            conduit.MinMaxAirlockDamageMultiplier.Y),
                        origin: ent);
                }
                else if (_tag.HasTag(ent, "Window"))
                {
                    _audio.PlayPvs(conduit.WindowDamageSound, Transform(ent).Coordinates);
                    ignored.Add(ent);
                    affected.Add(ent);
                    _damage.TryChangeDamage(ent,
                        dmg * _rand.NextFloat(conduit.MinMaxWindowDamageMultiplier.X,
                            conduit.MinMaxWindowDamageMultiplier.Y),
                        origin: ent);
                }
            }

            if (affected.Count > 0)
                _color.RaiseEffect(Color.Black, affected, Filter.Pvs(uid, 3f, EntityManager));
        }
    }

    private void Cycle(Entity<AristocratComponent, TransformComponent> ent)
    {
        if (TryComp(ent, out PhysicsComponent? physics))
            _physics.SetBodyStatus(ent.Owner, physics, BodyStatus.InAir);

        if (ent.Comp1.HasDied) // powers will only take effect for as long as we're alive
            return;

        var step = ent.Comp1.UpdateStep;

        if (step % 100 == 0)
        {
            step = 10;
        }

        if (step % 10 == 0)
            FreezeNoobs(ent);

        switch (step % 4)
        {
            case 0:
                ExtinguishFires(ent);
                break;
            case 1:
                FreezeAtmos((ent.Owner, ent.Comp2));
                break;
            case 2:
                DoChristmas(ent);
                break;
            case 3:
                SpookyLights(ent);
                break;
        }

        ent.Comp1.UpdateStep++;
    }

    private void FreezeBullet(Entity<AristocratComponent, TransformComponent?, PhysicsComponent?> ent,
        Entity<ProjectileComponent, AffectedByVoidAuraComponent, PhysicsComponent> bullet)
    {
        if (!Resolve(ent, ref ent.Comp2, ref ent.Comp3, false))
            return;

        var (uid, proj, affected, physics) = bullet;

        if (proj.Shooter == ent)
            return;

        var (pos, rot) = _xform.GetWorldPositionRotation(uid);

        var dir = pos - _xform.GetWorldPosition(ent.Comp2);
        var targetVelocity = dir.Length();

        if (targetVelocity > ent.Comp1.Range)
            return;

        // antihoming
        var homing = EnsureComp<HomingProjectileComponent>(uid);
        homing.Target = ent;
        var multiplier = MathHelper.Lerp(10f, 0f, Math.Clamp(targetVelocity * 2f / ent.Comp1.Range, 0f, 1f));
        homing.HomingSpeed = -128f * multiplier;
        Dirty(uid, homing);

        affected.OldVelocity ??= physics.LinearVelocity.Length();
        var oldLength = affected.OldVelocity.Value;

        var curVelocity = physics.LinearVelocity.Length();

        if (curVelocity < 0.01f)
            return;

        var dot = Vector2.Dot(dir, rot.ToWorldVec()) +
                  Vector2.Dot(dir, rot.ToVec()) +
                  Vector2.Dot(dir, rot.Opposite().ToVec());
        var modifier = MathF.Max(0f, dot);

        // If heretic is lying down, walking or moving slowly, bullets are slowed down even more
        var waltzMultiplier = TryComp(ent, out InputMoverComponent? mover) && !mover.Sprinting ||
            _standing.IsDown(ent) || ent.Comp3.LinearVelocity.Length() <= 2.5f
            ? 1f
            : 2f;

        targetVelocity = MathF.Max(5f, targetVelocity * (2f * waltzMultiplier + modifier * oldLength));
        targetVelocity = MathF.Min(targetVelocity, oldLength);

        if (MathF.Abs(curVelocity - targetVelocity) < 0.01f)
            return;

        _physics.SetLinearVelocity(uid, physics.LinearVelocity / curVelocity * targetVelocity, body: physics);
    }

    // makes shit cold
    private void FreezeAtmos(Entity<TransformComponent> ent)
    {
        var mix = _atmos.GetTileMixture((ent, Transform(ent)));
        var freezingTemp = Atmospherics.T0C;

        if (mix != null)
        {
            if (mix.Temperature > freezingTemp)
                mix.Temperature = freezingTemp;

            mix.Temperature -= 100f;
        }
    }

    // extinguish gases on tiles
    private void ExtinguishFiresTiles(Entity<AristocratComponent, TransformComponent> ent)
    {
        var coords = ent.Comp2.Coordinates;
        var tiles = GetTiles(coords, (int) ent.Comp1.Range);

        if (tiles.Count == 0)
            return;

        foreach (var pos in tiles)
        {
            var tile = _turf.GetTileRef(pos);

            if (tile == null)
                continue;

            _atmos.HotspotExtinguish(tile.Value.GridUid, tile.Value.GridIndices);
        }
    }

    // extinguish ppl and stuff
    private void ExtinguishFires(Entity<AristocratComponent, TransformComponent> ent)
    {
        var coords = ent.Comp2.Coordinates;
        var fires = _lookup.GetEntitiesInRange<FlammableComponent>(coords, ent.Comp1.Range);

        foreach (var (uid, flam) in fires)
        {
            if (flam.OnFire)
                _flammable.Extinguish(uid, flam);
        }

        ExtinguishFiresTiles(ent);
    }

    // replaces certain things with their winter analogue (amongst other things)
    private void DoChristmas(Entity<AristocratComponent, TransformComponent> ent)
    {
        SpawnTiles(ent);

        var coords = ent.Comp2.Coordinates;

        var tags = _lookup.GetEntitiesInRange<TagComponent>(coords, ent.Comp1.Range, LookupFlags.Static);

        foreach (var (uid, tag) in tags)
        {
            // walls
            if (!_tag.HasTag(tag, "Wall") || !_rand.Prob(.45f) ||
                (Prototype(uid)?.ID ?? string.Empty) == IceWallPrototype)
                continue;

            Spawn(IceWallPrototype, Transform(uid).Coordinates);
            QueueDel(uid);
        }
    }

    // kill the lights
    private void SpookyLights(Entity<AristocratComponent, TransformComponent> ent)
    {
        var coords = ent.Comp2.Coordinates;
        var lights = _lookup.GetEntitiesInRange<PoweredLightComponent>(coords, ent.Comp1.Range, LookupFlags.Static);

        foreach (var (uid, light) in lights)
        {
            _light.TryDestroyBulb(uid, light);
        }
    }

    // curses noobs
    private void FreezeNoobs(Entity<AristocratComponent, TransformComponent> ent)
    {
        var coords = ent.Comp2.Coordinates;
        var noobs = _lookup.GetEntitiesInRange<MobStateComponent>(coords, ent.Comp1.Range);

        foreach (var noob in noobs)
        {
            // Apply up to 3 void chill stacks
            _voidcurse.DoCurse(noob, 1, 3);
        }
    }

    private void SpawnTiles(Entity<AristocratComponent, TransformComponent> ent)
    {
        if (!TryComp<MapGridComponent>(ent.Comp2.GridUid, out var grid))
            return;

        var tiles = GetTiles(ent.Comp2.Coordinates, (int) ent.Comp1.Range);

        if (tiles.Count == 0)
            return;

        // it's christmas!!
        foreach (var pos in tiles)
        {
            if (!_rand.Prob(.3f))
                continue;

            var tile = _turf.GetTileRef(pos);

            if (tile == null)
                continue;

            var newTile = _prot.Index(SnowTilePrototype);
            _tile.ReplaceTile(tile.Value, newTile);

            var condition = _lookup.GetEntitiesInRange(pos, .1f, LookupFlags.Static | LookupFlags.Sensors)
                .All(e => Prototype(e)?.ID != IceTilePrototype.Id);
            if (condition)
                Spawn(IceTilePrototype, pos);
        }
    }
}
