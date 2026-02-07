// SPDX-FileCopyrightText: 2025 GoobBot
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Stack;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Production machines for the Doodon village.
/// FIXED: Warriors spawned by machines now immediately inherit Papa's CURRENT ORDER
/// (blackboard + component sync + follow policy + ordered target clearing + HTN replan),
/// so they behave correctly even if they spawn after Papa already chose an order.
/// </summary>
public sealed class DoodonMachineSystem : EntitySystem
{
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly SharedAudioSystem _soundSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HTNSystem _htn = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DoodonMachineComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<DoodonMachineComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<DoodonMachineComponent>();

        while (query.MoveNext(out var uid, out var machine))
        {
            // Spawn-on-placement logic
            if (machine.SpawnOnMapInit && !machine.InitialSpawnDone)
            {
                var isTownHall = HasComp<DoodonTownHallComponent>(uid);

                if (isTownHall || (TryComp<DoodonBuildingComponent>(uid, out var b) && b.Active))
                {
                    if (CanProduce(uid, machine))
                    {
                        Produce(uid, machine);
                        machine.InitialSpawnDone = true;
                    }
                }
            }

            // Processing / cooldown
            if (!machine.Processing)
                continue;

            machine.Accumulator += frameTime;
            if (machine.Accumulator < machine.ProcessTime)
                continue;

            machine.Accumulator = 0f;
            machine.Processing = false;

            // Final gate (in case housing changed during processing)
            if (CanProduce(uid, machine))
                Produce(uid, machine);
        }
    }

    private void OnInteractUsing(EntityUid uid, DoodonMachineComponent machine, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        var isTownHall = HasComp<DoodonTownHallComponent>(uid);
        if (!isTownHall &&
            TryComp<DoodonBuildingComponent>(uid, out var building) &&
            !building.Active)
            return;

        if (machine.Processing)
        {
            _popup.PopupEntity(Loc.GetString("doodon-machine-busy"), uid, args.User);
            return;
        }

        if (!TryComp<StackComponent>(args.Used, out var stack))
            return;

        if (stack.StackTypeId != machine.ResinStack)
            return;

        // Must be able to produce BEFORE we eat resin
        if (!CanProduce(uid, machine))
        {
            _popup.PopupEntity(Loc.GetString("doodon-machine-cannot-produce"), uid, args.User);
            return;
        }

        // Must insert full cost at once
        if (stack.Count < machine.ResinCost)
        {
            _popup.PopupEntity(
                Loc.GetString("doodon-machine-not-enough-resin",
                    ("current", stack.Count),
                    ("required", machine.ResinCost)),
                uid,
                args.User);
            return;
        }

        if (!_stackSystem.Use(args.Used, machine.ResinCost))
            return;

        _soundSystem.PlayPvs(
            machine.InsertSound,
            Transform(uid).Coordinates,
            AudioParams.Default.WithPitchScale(_random.NextFloat(0.92f, 1.08f)));

        _popup.PopupEntity(Loc.GetString("doodon-machine-insert-resin"), uid, args.User);

        machine.Processing = true;
        machine.Accumulator = 0f;

        args.Handled = true;
    }

    private bool CanProduce(EntityUid machineUid, DoodonMachineComponent machine)
    {
        // Town hall can always do its papa spawn etc (if you want)
        if (HasComp<DoodonTownHallComponent>(machineUid))
            return true;

        if (!machine.RespectPopulationCap)
            return true;

        if (!TryComp<DoodonBuildingComponent>(machineUid, out var building) ||
            !building.Active || building.TownHall is not { } hallUid)
            return false;

        if (!TryComp<DoodonTownHallComponent>(hallUid, out var hall))
            return false;

        // If this output doesn't consume housing, always ok
        if (machine.ProducedHousing == DoodonHousingType.None)
            return true;

        GetHousingStats(hallUid, hall,
            out var workerCap, out var warriorCap, out var moodonCap,
            out var workerPop, out var warriorPop, out var moodonPop);

        return machine.ProducedHousing switch
        {
            DoodonHousingType.Worker => workerPop < workerCap,
            DoodonHousingType.Warrior => warriorPop < warriorCap,
            DoodonHousingType.Moodon => moodonPop < moodonCap,
            _ => true
        };
    }

    private void GetHousingStats(
        EntityUid hallUid, DoodonTownHallComponent hall,
        out int workerCap, out int warriorCap, out int moodonCap,
        out int workerPop, out int warriorPop, out int moodonPop)
    {
        workerCap = warriorCap = moodonCap = 0;
        workerPop = warriorPop = moodonPop = 0;

        // Capacity from buildings
        foreach (var b in hall.Buildings)
        {
            if (Deleted(b) || !TryComp<DoodonBuildingComponent>(b, out var building))
                continue;

            if (!building.Active || building.TownHall != hallUid)
                continue;

            switch (building.HousingType)
            {
                case DoodonHousingType.Worker:
                    workerCap += building.HousingCapacity;
                    break;
                case DoodonHousingType.Warrior:
                    warriorCap += building.HousingCapacity;
                    break;
                case DoodonHousingType.Moodon:
                    moodonCap += building.HousingCapacity;
                    break;
            }
        }

        // Occupancy from doodons
        foreach (var d in hall.Doodons)
        {
            if (Deleted(d) || !TryComp<DoodonComponent>(d, out var doodon))
                continue;

            switch (doodon.RequiredHousing)
            {
                case DoodonHousingType.Worker:
                    workerPop++;
                    break;
                case DoodonHousingType.Warrior:
                    warriorPop++;
                    break;
                case DoodonHousingType.Moodon:
                    moodonPop++;
                    break;
            }
        }
    }

    private void Produce(EntityUid machineUid, DoodonMachineComponent machine)
    {
        var coords = Transform(machineUid).Coordinates;
        var spawned = Spawn(machine.OutputPrototype, coords);

        // If warrior: wire follow + orders from nearest papa (FULL SYNC)
        if (TryComp<DoodonWarriorComponent>(spawned, out var warriorComp))
        {
            if (TryGetPapaForMachine(machineUid, out var papaUid))
            {
                warriorComp.Papa = papaUid;

                if (TryComp<PapaDoodonComponent>(papaUid, out var papaComp))
                {
                    // 1) Orders: blackboard + component sync
                    _npc.SetBlackboard(spawned, NPCBlackboard.CurrentOrders, papaComp.CurrentOrder);
                    warriorComp.Orders = papaComp.CurrentOrder;
                    Dirty(spawned, warriorComp);

                    // 2) Clear ordered target unless AttackTarget
                    if (papaComp.CurrentOrder != DoodonOrderType.AttackTarget)
                        _npc.SetBlackboard(spawned, NPCBlackboard.CurrentOrderedTarget, EntityUid.Invalid);

                    // 3) Follow policy identical to Papa system
                    var papaFollowCoords = new EntityCoordinates(papaUid, Vector2.Zero);

                    if (papaComp.CurrentOrder == DoodonOrderType.Follow ||
                        papaComp.CurrentOrder == DoodonOrderType.AttackTarget)
                        _npc.SetBlackboard(spawned, NPCBlackboard.FollowTarget, papaFollowCoords);
                    else
                        _npc.SetBlackboard(spawned, NPCBlackboard.FollowTarget, default(EntityCoordinates));
                }
                else
                {
                    // Fallback: at least follow papa
                    _npc.SetBlackboard(spawned, NPCBlackboard.FollowTarget, new EntityCoordinates(papaUid, Vector2.Zero));
                }

                // 4) Force HTN to select correct branch immediately
                if (TryComp<HTNComponent>(spawned, out var htnComp))
                {
                    if (htnComp.Plan != null)
                        _htn.ShutdownPlan(htnComp);

                    _htn.Replan(htnComp);
                }
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

    private void OnExamined(EntityUid uid, DoodonMachineComponent machine, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var connected =
            HasComp<DoodonTownHallComponent>(uid) ||
            (TryComp<DoodonBuildingComponent>(uid, out var building) && building.TownHall != null);

        args.PushMarkup(Loc.GetString(connected
            ? "doodon-machine-connected"
            : "doodon-machine-not-connected"));
    }
}
