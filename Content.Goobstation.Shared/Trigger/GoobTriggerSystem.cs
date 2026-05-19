using System.Linq;
using Content.Shared.Ensnaring;
using Content.Shared.Ensnaring.Components;
using Content.Shared.EntityTable;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Stunnable;
using Content.Shared.Trigger;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Trigger;

public sealed class GoobTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedEnsnareableSystem _snare = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParalyzePullerOnTriggerComponent, TriggerEvent>(OnParalyzeTrigger);
        SubscribeLocalEvent<RemoveSnareOnTriggerComponent, TriggerEvent>(OnSnareTrigger);
        SubscribeLocalEvent<SpawnTableOnTriggerComponent, TriggerEvent>(OnSpawnerTrigger);
        SubscribeLocalEvent<TriggerCounterComponent, TriggerEvent>(OnTriggerCounter);
        SubscribeLocalEvent<TriggerCounterLimitComponent, AttemptTriggerEvent>(OnTriggerLimitCounter);
    }

    private void OnSnareTrigger(Entity<RemoveSnareOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!TryComp<EnsnareableComponent>(target, out var ensnareable) || !ensnareable.IsEnsnared ||
            ensnareable.Container.ContainedEntities.Count <= 0)
            return;

        var bola = ensnareable.Container.ContainedEntities[0];
        _snare.ForceFree(bola, Comp<EnsnaringComponent>(bola));
        args.Handled = true;
    }

    private void OnParalyzeTrigger(Entity<ParalyzePullerOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!TryComp<PullableComponent>(target, out var pullable) || !pullable.Puller.HasValue)
            return;

        _stun.TryUpdateParalyzeDuration(pullable.Puller.Value, TimeSpan.FromSeconds(5));
        args.Handled = true;
    }

    private void OnSpawnerTrigger(Entity<SpawnTableOnTriggerComponent> ent, ref TriggerEvent args)
    {
        var target = ent.Comp.TargetUser ? args.User : ent.Owner;
        if (target == null)
            return;

        var xform = Transform(target.Value);
        var spawns = _entityTable.GetSpawns(ent.Comp.Table, _random.GetRandom()).ToList();

        if (ent.Comp.UseMapCoords)
        {
            var mapCoords = _transform.GetMapCoordinates(target.Value, xform);
            if (ent.Comp.Predicted)
            {
                foreach (var spawn in spawns)
                {
                    EntityManager.PredictedSpawn(spawn, mapCoords);
                }
            }
            else if (_net.IsServer)
            {
                foreach (var spawn in spawns)
                {
                    Spawn(spawn, mapCoords);
                }
            }
        }
        else
        {
            var coords = xform.Coordinates;
            if (!coords.IsValid(EntityManager))
                return;

            if (ent.Comp.Predicted)
            {
                foreach (var spawn in spawns)
                {
                    PredictedSpawnAttachedTo(spawn, coords);
                }
            }
            else if (_net.IsServer)
            {
                foreach (var spawn in spawns)
                {
                    SpawnAttachedTo(spawn, coords);
                }
            }
        }
    }

    private void OnTriggerCounter(Entity<TriggerCounterComponent> ent, ref TriggerEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Count++;
    }

    private void OnTriggerLimitCounter(Entity<TriggerCounterLimitComponent> ent, ref AttemptTriggerEvent args)
    {
        if (!TryComp(ent.Owner, out TriggerCounterComponent? comp))
            return;

        if (comp.Count >= ent.Comp.MaxCount)
            args.Cancelled = true;
    }
}
