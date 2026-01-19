using System.Numerics;
using Content.Goobstation.Common.Physics;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems.Abilities;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedStarGazerSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;

    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedHereticAbilitySystem _hereticAbility = default!;
    [Dependency] private readonly SharedHereticSystem _heretic = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStarMarkSystem _starMark = default!;

    protected const string JointId = "stargaze";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StarGazerComponent, StarGazeEvent>(OnStarGaze);
        SubscribeLocalEvent<StarGazerComponent, MeleeHitEvent>(OnStarGazerHit);

        SubscribeLocalEvent<StarGazeComponent, StarGazeDoAfterEvent>(OnStarGazeDoAfter);
        SubscribeLocalEvent<StarGazeComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<StarGazeComponent, ComponentStartup>(OnStarGazeStartup);
        SubscribeLocalEvent<StarGazeComponent, ComponentShutdown>(OnStarGazeShutdown);
        SubscribeLocalEvent<StarGazeComponent, AttackAttemptEvent>(OnStarGazeAttackAttempt);

        SubscribeAllEvent<LaserBeamEndpointPositionEvent>(OnGetPosition);
    }

    private void OnStarGazerHit(Entity<StarGazerComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var uid in args.HitEntities)
        {
            _starMark.TryApplyStarMark(uid);
        }
    }

    private void OnStarGazeAttackAttempt(Entity<StarGazeComponent> ent, ref AttackAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnGetPosition(LaserBeamEndpointPositionEvent ev)
    {
        if (!TryGetEntity(ev.Uid, out var uid))
            return;

        if (!TryComp(uid.Value, out StarGazeComponent? starGaze) || !TryComp(uid.Value, out TransformComponent? xform))
            return;

        if (xform.MapID != ev.Coordinates.MapId)
            return;

        var pos = ev.Coordinates;
        var ourPos = Xform.GetWorldPosition(xform);

        var dir = pos.Position - ourPos;
        var len = dir.Length();
        var newLen = Math.Clamp(len, starGaze.MinMaxLaserRange.X, starGaze.MinMaxLaserRange.Y);
        if (Math.Abs(len - newLen) > 0.01f)
            pos = new MapCoordinates(ourPos + dir * newLen / len, xform.MapID);

        if (starGaze.CursorPosition == pos)
            return;

        starGaze.CursorPosition = pos;
        Dirty(uid.Value, starGaze);
    }

    protected virtual void OnStarGazeShutdown(Entity<StarGazeComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent))
            _movement.RefreshMovementSpeedModifiers(ent);

        if (Exists(ent.Comp.BeamSoundEnt))
            PredictedQueueDel(ent.Comp.BeamSoundEnt);
    }

    protected virtual void OnStarGazeStartup(Entity<StarGazeComponent> ent, ref ComponentStartup args)
    {
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshMovespeed(Entity<StarGazeComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.LifeStage < ComponentLifeStage.Stopping)
            args.ModifySpeed(ent.Comp.Slowdown.X, ent.Comp.Slowdown.Y, true);
    }

    private void OnStarGazeDoAfter(Entity<StarGazeComponent> ent, ref StarGazeDoAfterEvent args)
    {
        var (uid, comp) = ent;

        if (args.Cancelled || args.Handled || comp.CursorPosition == null)
        {
            if (TryGetEntity(args.OrbEffect, out var orb) && Exists(orb.Value))
                PredictedQueueDel(orb.Value);

            RemCompDeferred(uid, comp);
            return;
        }

        if (comp.CursorPosition.Value.MapId != Transform(uid).MapID)
        {
            RemCompDeferred(uid, comp);
            return;
        }

        args.Handled = true;

        if (_net.IsServer)
        {
            comp.Endpoint = Spawn(null, Xform.ToCoordinates(comp.CursorPosition.Value));
            var endpoint = comp.Endpoint.Value;
            EnsureComp<LaserBeamEndpointComponent>(endpoint);
            EnsureComp<TimedDespawnComponent>(endpoint).Lifetime = comp.LaserDuration;
            var beam = EnsureComp<ComplexJointVisualsComponent>(uid);
            var data = new ComplexJointVisualsData(JointId, comp.Beam1, comp.Start1, comp.End1, Timing.CurTime)
            {
                Scale = new Vector2(comp.BeamScale),
            };
            beam.Data[GetNetEntity(endpoint)] = data;

            comp.BeamSoundEnt = _audio.PlayEntity(ent.Comp.BeamSound,
                    Filter.Empty().AddInMap(comp.CursorPosition.Value.MapId, EntityManager),
                    uid,
                    true)
                ?.Entity;
        }

        comp.StartedBlasting = true;
        Dirty(ent);
    }

    private void OnStarGaze(Entity<StarGazerComponent> ent, ref StarGazeEvent args)
    {
        if (!_hereticAbility.TryUseAbility(args, false))
            return;

        var orbEffect = PredictedSpawnAttachedTo(args.OrbEffect, ent.Owner.ToCoordinates());

        var doArgs = new DoAfterArgs(EntityManager,
            ent,
            args.DoAfterDelay,
            new StarGazeDoAfterEvent(GetNetEntity(orbEffect)),
            ent)
        {
            BreakOnHandChange = false,
            RequireCanInteract = false,
            MultiplyDelay = false,
        };

        if (!_doAfter.TryStartDoAfter(doArgs))
        {
            PredictedQueueDel(orbEffect);
            return;
        }

        EnsureComp<StarGazeComponent>(ent);
        _audio.PlayPredicted(args.BeamStartSound, ent, ent);

        args.Handled = true;
    }

    public Entity<StarGazerComponent>? ResolveStarGazer(Entity<CosmosPassiveComponent?> summoner,
        out bool spawned,
        bool checkAscend = true,
        EntityCoordinates? spawnCoords = null)
    {
        spawned = false;

        if (!Resolve(summoner, ref summoner.Comp, false) ||
            !_heretic.TryGetHereticComponent(summoner, out var heretic, out var mind) ||
            heretic.CurrentPath != "Cosmos" || checkAscend && !heretic.Ascended)
            return null;

        StarGazerComponent? comp;
        HereticMinionComponent? minion;

        var starGazer = summoner.Comp.StarGazer;
        if (!Exists(starGazer))
            starGazer = heretic.Minions.FirstOrNull(x => Exists(x) && HasComp<StarGazerComponent>(x));

        if (starGazer == null)
        {
            starGazer = PredictedSpawnAtPosition(summoner.Comp.StarGazerId,
                spawnCoords ?? Transform(summoner).Coordinates);
            Xform.AttachToGridOrMap(starGazer.Value);
            comp = EnsureComp<StarGazerComponent>(starGazer.Value);
            minion = EnsureComp<HereticMinionComponent>(starGazer.Value);
            minion.BoundHeretic = summoner;
            summoner.Comp.StarGazer = starGazer.Value;
            heretic.Minions.Add(starGazer.Value);
            Dirty(mind, heretic);
            Dirty(summoner, summoner.Comp);
            Dirty(starGazer.Value, minion);
            spawned = true;
            return (starGazer.Value, comp);
        }

        heretic.Minions.Add(starGazer.Value);
        Dirty(mind, heretic);

        comp = EnsureComp<StarGazerComponent>(starGazer.Value);

        if (EnsureComp<HereticMinionComponent>(starGazer.Value, out minion) &&
            minion.BoundHeretic == summoner.Owner)
            return (starGazer.Value, comp);

        minion.BoundHeretic = summoner.Owner;
        Dirty(starGazer.Value, minion);

        return (starGazer.Value, comp);
    }
}
