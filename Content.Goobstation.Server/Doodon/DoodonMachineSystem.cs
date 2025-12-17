using Content.Server.Stack;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Goobstation.Shared.Doodons;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Doodons;

public sealed class DoodonMachineSystem : EntitySystem
{
    [Dependency] private readonly StackSystem _stackSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DoodonMachineComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<DoodonMachineComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<DoodonMachineComponent>();

        while (query.MoveNext(out var uid, out var machine))
        {
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

    private void OnMapInit(EntityUid uid, DoodonMachineComponent machine, ref MapInitEvent args)
    {
        if (!machine.SpawnOnMapInit)
            return;

        if (!CanProduce(uid, machine))
            return;

        Produce(uid, machine);
    }

    /// <summary>
    /// Insert resin from hand.
    /// </summary>
    private void OnInteractUsing(EntityUid uid, DoodonMachineComponent machine, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<StackComponent>(args.Used, out var stack))
            return;

        if (stack.StackTypeId != machine.ResinStack)
            return;

        var needed = machine.ResinCost - machine.StoredResin;
        if (needed <= 0)
            return;

        var toTake = Math.Min(needed, stack.Count);

        if (!_stackSystem.Use(args.Used, toTake))
            return;

        machine.StoredResin += toTake;

        // Start processing once enough resin is inserted
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
        if (!machine.RespectPopulationCap)
            return true;

        if (!TryComp<DoodonBuildingComponent>(uid, out var building))
            return true;

        if (building.TownHall is not { } hallUid)
            return true;

        if (!TryComp<DoodonTownHallComponent>(hallUid, out var hall))
            return true;

        return hall.CanSpawnMoreDoodons;
    }

    private void Produce(EntityUid uid, DoodonMachineComponent machine)
    {
        Spawn(machine.OutputPrototype, Transform(uid).Coordinates);
    }
}
