// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Vapor;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.OfficeChair;

public sealed partial class RocketChairSystem : SharedRocketChairSystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _tx = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly VaporSystem _vapor = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RocketChairComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RocketChairComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.BoostEnd)
                continue;

            if (!_solutions.TryGetSolution(uid, comp.FuelSolution, out var solnEnt, out var solution))
            {
                comp.BoostEnd = _timing.CurTime;
                continue;
            }

            var have = solution.GetTotalPrototypeQuantity(comp.FuelReagent);
            if (have <= FixedPoint2.Zero)
            {
                comp.BoostEnd = _timing.CurTime;
                continue;
            }

            var need = FixedPoint2.New(comp.FuelPerSecond * MathF.Max(frameTime, 0f));
            var take = FixedPoint2.Min(have, need);
            if (take > FixedPoint2.Zero)
                _solutions.RemoveReagent(solnEnt!.Value, comp.FuelReagent, take);

            if (have < need)
                comp.BoostEnd = _timing.CurTime;

            comp.EmitElapsed += TimeSpan.FromSeconds(MathF.Max(frameTime, 0f));
            var emits = 0;
            var interval = TimeSpan.FromSeconds(comp.EmitInterval);
            while (comp.EmitElapsed >= interval && emits < comp.EmitMaxPerTick)
            {
                SpawnVaporBurst(uid, comp);
                comp.EmitElapsed -= interval;
                emits++;
            }
        }
    }

    private void OnMapInit(Entity<RocketChairComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;

        _solutions.EnsureSolutionEntity((uid, (SolutionContainerManagerComponent?) null),
            comp.FuelSolution, out _, out var solEnt, FixedPoint2.New(comp.FuelCapacity));

        if (solEnt != null && solEnt.Value.Comp.Solution.Volume == 0 && comp.StartFuel > 0)
            _solutions.TryAddReagent(solEnt.Value, comp.FuelReagent, FixedPoint2.New(comp.StartFuel));
    }

    private void SpawnVaporBurst(EntityUid uid, RocketChairComponent comp)
    {
        if (!_solutions.TryGetSolution(uid, comp.FuelSolution, out Entity<SolutionComponent>? solnEnt, out var fuelSol))
            return;

        var color = _proto.Index<ReagentPrototype>(comp.FuelReagent).SubstanceColor.WithAlpha(1f);

        var chairPos = _tx.GetMapCoordinates(uid);
        var dir = -comp.BoostDir;
        var baseAngle = dir.ToAngle();
        var nozzle = chairPos.Offset(dir * comp.NozzleOffset);

        var count = Math.Max(1, comp.VaporCountPerEmit);
        var half = comp.VaporSpread * 0.5f;

        var perPuff = FixedPoint2.New(comp.VaporReagentPerPuff);

        for (var i = 0; i < count; i++)
        {
            var extracted = _solutions.SplitSolution(solnEnt!.Value, perPuff);
            if (extracted.Volume <= FixedPoint2.Zero)
                break;

            var t = count <= 1 ? 0f : i / (float) (count - 1);
            var off = -half + t * comp.VaporSpread;
            var rot = baseAngle + Angle.FromDegrees(off);
            var vdir = rot.ToVec();

            var vapor = Spawn(comp.VaporPrototype, nozzle);
            var vx = Transform(vapor);
            _tx.SetWorldRotation(vx, rot);

            if (TryComp(vapor, out AppearanceComponent? app))
            {
                _appearance.SetData(vapor, VaporVisuals.Color, color, app);
                _appearance.SetData(vapor, VaporVisuals.State, true, app);
            }

            var vap = Comp<VaporComponent>(vapor);
            var vapEnt = (vapor, vap);
            _vapor.TryAddSolution(vapEnt, extracted);

            var speed = comp.VaporVelocity;
            var life = comp.VaporLifetime;
            var target = nozzle.Offset(vdir * (speed * life));

            _vapor.Start(vapEnt, vx,
                vdir * speed * life,
                speed,
                target,
                life,
                comp.LastPilot ?? uid);
        }

        _audio.PlayPvs(comp.SpraySound, uid, comp.SpraySound.Params.WithVariation(0.125f));
    }
}
