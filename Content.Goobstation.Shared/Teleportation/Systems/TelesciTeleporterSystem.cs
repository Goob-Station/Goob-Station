using System.Numerics;
using Content.Goobstation.Common.BlockTeleport;
using Content.Goobstation.Shared.Teleportation.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DeviceLinking;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Teleportation.Systems;

public sealed class TelesciTeleporterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelesciTeleporterComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TelesciTeleporterComponent, TelesciSendEvent>(OnSendEvent);
        SubscribeLocalEvent<TelesciTeleporterComponent, TelesciRetrieveEvent>(OnRetriveEvent);
    }

    private void OnMapInit(Entity<TelesciTeleporterComponent> ent, ref MapInitEvent arg)
    {
        if (!TryComp<DeviceLinkSinkComponent>(ent, out var sink))
            return;

        foreach (var source in sink.LinkedSources)
        {
            if (!TryComp<TelesciComputerComponent>(source, out var computer))
                continue;

            computer.TeleporterEntity = GetNetEntity(ent);
            ent.Comp.Computer = source;
            Dirty(source, computer);
            Dirty(ent);
            break;
        }
    }

    private void OnSendEvent(Entity<TelesciTeleporterComponent> ent, ref TelesciSendEvent arg)
    {
        if (!_power.IsPowered(ent.Owner))
            return;

        if (ent.Comp.Cooldown > _time.CurTime)
            return;

        StartCooldown(ent);

        var cords = _xform.GetMapCoordinates(ent);
        var newCords = ScrambleVector(ent, arg.Coordinates);

        if (Vector2.Distance(cords.Position, newCords) > ent.Comp.TeleportMaxDistance)
            return;

        Teleport(ent,cords.Position, newCords);
        Dirty(ent);
    }

    private void OnRetriveEvent(Entity<TelesciTeleporterComponent> ent, ref TelesciRetrieveEvent arg)
    {
        if (!_power.IsPowered(ent.Owner))
            return;

        if (ent.Comp.Cooldown > _time.CurTime)
            return;

        StartCooldown(ent);

        var coords = _xform.GetMapCoordinates(ent);
        var newCoords = ScrambleVector(ent, arg.Coordinates);

        if (Vector2.Distance(coords.Position, newCoords) > ent.Comp.TeleportMaxDistance)
            return;

        Teleport(ent,newCoords, coords.Position );
        Dirty(ent);
    }

    private void Teleport(Entity<TelesciTeleporterComponent> ent, Vector2 location, Vector2 target)
    {
        var entitiesToTeleport= _lookup.GetEntitiesInRange(
            Transform(ent).MapID,
            location,
            ent.Comp.TeleportSize,
            LookupFlags.Uncontained);

        if (entitiesToTeleport.Count < 1)
            return;

        var list = new List<EntityUid>();
        foreach (var n in entitiesToTeleport)
        {
            if (Transform(n).Anchored)
                continue;

            if (!CanTeleport(n))
                continue;

            list.Add(n);
        }

        if (list.Count > 1) // Chance of Failure if more than one item //TODO this is shit change it
        {
            var random = _random.Next(0, 100); // low is bad

            if (random == 0) // automatic Failure
            {
                TeleportFailure(ent, target, list);
                return;
            }

            if (random == 99) // Automatic Success
            {
                TeleportSuccess(ent, target, list);
                return;
            }

            var probability = list.Count * ent.Comp.TeleportFaliureMultiplyer;

            if (random < probability)
                TeleportFailure(ent, target, list);
            else
                TeleportSuccess(ent, target, list);
        }
        else
            TeleportSuccess(ent, target, list);
    }

    private void TeleportSuccess(Entity<TelesciTeleporterComponent> ent, Vector2 target, List<EntityUid> entitiesToTeleport)
    {
        if (entitiesToTeleport.Count < 1)
            return;

        var thisOne =_random.Next(0, entitiesToTeleport.Count);

        _pullingSystem.StopAllPulls(entitiesToTeleport[thisOne]);
        SpawnAtPosition("EffectTelesciTeleportation", Transform(entitiesToTeleport[thisOne]).Coordinates);
        _audio.PlayPvs(ent.Comp.SoundSucess, Transform(entitiesToTeleport[thisOne]).Coordinates);

        _xform.SetWorldPosition(entitiesToTeleport[thisOne], target);

        SpawnAttachedTo("EffectTelesciTeleportation", Transform(entitiesToTeleport[thisOne]).Coordinates);
        _audio.PlayPvs(ent.Comp.SoundSucess, Transform(entitiesToTeleport[thisOne]).Coordinates);
    }

    private void TeleportFailure(Entity<TelesciTeleporterComponent> ent, Vector2 target, List<EntityUid> entitiesToTeleport)
    {
        switch (_random.Next(0, 100))  // TODO MOVE THIS TO YAML SOMEHOW
        {
            case <25: // one goliath
                PredictedSpawnAtPosition("EffectTelesciTeleportation", Transform(ent).Coordinates);
                _audio.PlayPvs(ent.Comp.SoundFaliure, Transform(ent).Coordinates);
                SpawnAtPosition("MobLavalandGoliath", Transform(ent).Coordinates);
                break;

            case <50: // a random number of carps
                var carpAmount = Math.Min(_random.Next(1, entitiesToTeleport.Count), 10); // max 10 carps

                PredictedSpawnAtPosition("EffectTelesciTeleportation", Transform(ent).Coordinates);
                _audio.PlayPvs(ent.Comp.SoundFaliure, Transform(ent).Coordinates);

                for (int i = 0; i < carpAmount; i++)
                {
                    var carp = PredictedSpawnAtPosition("MobCarpHolo", Transform(ent).Coordinates);
                    var despawn = EnsureComp<TimedDespawnComponent>(carp);
                    despawn.Lifetime = 60; // despawns after one minute
                }
                break;

            case >90: // gib all living targets and send teleporter to void
                SpawnAttachedTo("EffectTelesciTeleportation", Transform(ent).Coordinates);
                foreach (var gib in entitiesToTeleport)
                {
                    if (HasComp<MobStateComponent>(gib))
                    {
                        PredictedSpawnAtPosition("EffectTelesciTeleportation", Transform(gib).Coordinates);
                        _body.GibBody(gib);
                    }
                }
                _audio.PlayPvs(ent.Comp.SoundFaliure, Transform(ent).Coordinates);
                QueueDel(ent);
                break;

            default: // 50 - 90 is safe, 40% no bad stuff chance
                _audio.PlayPvs(ent.Comp.SoundFaliure, Transform(ent).Coordinates);
                break;
        }
    }

    private bool CanTeleport(EntityUid uid)
    {
        var ev = new TeleportAttemptEvent(false);
        RaiseLocalEvent(uid, ref ev);
        return !ev.Cancelled;
    }

    private Vector2 ScrambleVector(Entity<TelesciTeleporterComponent> ent, Vector2 input)
    {
        var coords = _xform.GetMapCoordinates(ent);

       return coords.Offset(input).Position;
    }

    private void StartCooldown(Entity<TelesciTeleporterComponent> ent)
    {
        ent.Comp.Cooldown = _time.CurTime + ent.Comp.CooldownInterval;

        if (ent.Comp.Computer == null)
            return;

        Dirty(ent);
        RaiseLocalEvent(ent.Comp.Computer.Value, new TelesciCooldowneEvent(ent.Comp.Cooldown));
    }
}
