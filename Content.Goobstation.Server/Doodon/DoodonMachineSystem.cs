using System.Numerics;
using Content.Server.Stack;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Doodons;

public sealed class DoodonMachineSystem : EntitySystem
{
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly NPCSystem _npc = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DoodonMachineComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<DoodonMachineComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, DoodonMachineComponent machine, ref MapInitEvent args)
    {
        // Important: do NOT spawn here.
        // Construction/placement causes MapInit before town hall assignment,
        // so we delay initial spawn until the building becomes Active.
        machine.InitialSpawnDone = false;
        machine.Processing = false;
        machine.Accumulator = 0f;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<DoodonMachineComponent>();

        while (query.MoveNext(out var uid, out var machine))
        {
            // ------------------------------------
            // Delayed "spawn on placement"
            // ------------------------------------
            if (machine.SpawnOnMapInit && !machine.InitialSpawnDone)
            {
                var isTownHall = HasComp<DoodonTownHallComponent>(uid);

                // Town Hall: always allowed to do its initial spawn (Papa)
                // Other buildings: wait until they're Active (assigned + in radius)
                if (isTownHall || (TryComp<DoodonBuildingComponent>(uid, out var b) && b.Active))
                {
                    if (CanProduce(uid, machine))
                    {
                        Produce(uid, machine);
                        machine.InitialSpawnDone = true;
                    }
                }
            }

            // ------------------------------------
            // Processing ticking
            // ------------------------------------
            if (!machine.Processing)
                continue;

            machine.Accumulator += frameTime;

            if (machine.Accumulator < machine.ProcessTime)
                continue;

            machine.Accumulator = 0f;
            machine.Processing = false;

            Produce(uid, machine);
        }
    }

    private void OnInteractUsing(EntityUid uid, DoodonMachineComponent machine, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Require active (except TownHall; you probably still want townhall usable always)
        var isTownHall = HasComp<DoodonTownHallComponent>(uid);
        if (!isTownHall && TryComp<DoodonBuildingComponent>(uid, out var building) && !building.Active)
            return;

        if (machine.Processing)
            return;

        if (!TryComp<StackComponent>(args.Used, out var stack))
            return;

        if (stack.StackTypeId != machine.ResinStack)
            return;

        var needed = machine.ResinCost - machine.StoredResin;
        if (needed <= 0)
            return;

        var toTake = Math.Min(needed, stack.Count);
        if (toTake <= 0)
            return;

        if (!_stackSystem.Use(args.Used, toTake))
            return;

        machine.StoredResin += toTake;

        // Start processing if enough resin and allowed to produce
        if (machine.StoredResin >= machine.ResinCost && CanProduce(uid, machine))
        {
            machine.StoredResin -= machine.ResinCost;
            machine.Processing = true;
            machine.Accumulator = 0f;
        }

        args.Handled = true;
    }

    private bool CanProduce(EntityUid uid, DoodonMachineComponent machine)
    {
        // Town hall should always be able to spawn its papa.
        if (HasComp<DoodonTownHallComponent>(uid))
            return true;

        if (!machine.RespectPopulationCap)
            return true;

        if (!TryComp<DoodonBuildingComponent>(uid, out var building))
            return false;

        if (!building.Active)
            return false;

        if (building.TownHall is not { } hallUid)
            return false;

        if (!TryComp<DoodonTownHallComponent>(hallUid, out var hall))
            return false;

        return hall.CanSpawnMoreDoodons;
    }

    private void Produce(EntityUid machineUid, DoodonMachineComponent machine)
    {
        var coords = Transform(machineUid).Coordinates;
        var spawned = Spawn(machine.OutputPrototype, coords);

        // If warrior: wire follow + orders from nearest papa
        if (TryComp<DoodonWarriorComponent>(spawned, out var warriorComp))
        {
            if (TryGetPapaForMachine(machineUid, out var papaUid))
            {
                warriorComp.Papa = papaUid;
                // DO NOT Dirty() unless DoodonWarriorComponent is NetworkedComponent.

                _npc.SetBlackboard(spawned, NPCBlackboard.FollowTarget, Transform(papaUid).Coordinates);

                if (TryComp<PapaDoodonComponent>(papaUid, out var papaComp))
                    _npc.SetBlackboard(spawned, NPCBlackboard.CurrentOrders, papaComp.CurrentOrder);
            }
        }
    }

    private bool TryGetPapaForMachine(EntityUid machineUid, out EntityUid papa)
    {
        papa = default;

        var xform = Transform(machineUid);
        var bestDist = float.MaxValue;

        var query = EntityQueryEnumerator<PapaDoodonComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var tf))
        {
            if (tf.MapID != xform.MapID)
                continue;

            var d = (tf.WorldPosition - xform.WorldPosition).LengthSquared();
            if (d >= bestDist)
                continue;

            bestDist = d;
            papa = uid;
        }

        return papa != default;
    }
}
