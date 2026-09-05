using Content.Shared._Lavaland.EntityShapes;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Spawns a ring of entities, increases radius, and spawns a new ring. Repeat X number of times.
/// Also anchors the megafauna, so this isn't really reusable.
/// </summary>
public sealed class CosmicRayCirculatorSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityShapeSystem _shapes = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicRayCirculatorComponent, CosmicRayCirculatorActionEvent>(OnAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // I'd make this a predicted spawn but MegafaunAnchor isn't predicted lol
        // Most Megafauna systems don't work with prediction, hurray!!! Thanks Obama (Rouden)
        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<CosmicRayCirculatorComponent, MegafaunaAnchorComponent>();
        while (query.MoveNext(out var uid, out var comp, out var anchor))
        {
            if (!comp.Active || _timing.CurTime < comp.NextWaveTime)
                continue;

            var waveRadius = comp.Radius + comp.RadiusIncrease * comp.CurrentWave;
            var shape = new RingEntityShape { Radius = waveRadius, Size = comp.Count };
            _shapes.SpawnEntityShape(shape, uid, comp.WarningPrototype, out _);

            comp.CurrentWave++;

            if (comp.CurrentWave >= comp.WaveCount)
            {
                // When all waves are done unachor the boss again.
                anchor.Anchored = false;
                comp.Active = false;
                comp.CurrentWave = 0;
            }
            else
            {
                // Start next wave of spawns
                comp.NextWaveTime = _timing.CurTime + comp.WaveDelay;
            }
        }
    }
    private void OnAction(EntityUid uid, CosmicRayCirculatorComponent comp, CosmicRayCirculatorActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_net.IsServer)
            return;

        // Don't want the boss moving during this cause it looks whack
        if (TryComp<MegafaunaAnchorComponent>(uid, out var anchor))
            anchor.Anchored = true;

        comp.Active = true;
        comp.CurrentWave = 0;
        comp.NextWaveTime = _timing.CurTime + comp.Delay;

        args.Handled = true;
    }
}
