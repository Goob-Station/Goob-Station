using Content.Goobstation.Shared.Wraith.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using Content.Goobstation.Shared.Wraith.Systems;

namespace Content.Goobstation.Server.Wraith.Systems;

public sealed class VoidPortalSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SummonPortalSystem _summonPortal = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidPortalComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VoidPortalComponent, ComponentShutdown>(OnComponentShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VoidPortalComponent>();
        while (query.MoveNext(out var uid, out var portal))
        {
            if (_timing.CurTime < portal.Accumulator)
                continue;

            UpdatePortal((uid, portal));
        }
    }

    private void OnMapInit(Entity<VoidPortalComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.CurrentPower = ent.Comp.ExtraPower; // start with X points
        TrySpawn(ent);

        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.SpawnInterval;
    }

    private void UpdatePortal(Entity<VoidPortalComponent> ent)
    {
        var portal = ent.Comp;

        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.SpawnInterval;

        // --- Wave Power Growth ---
        // Each wave, portal gains PowerGainPerTick + (ExtraPower * number of waves so far).
        // This makes later waves stronger.
        var gained = portal.PowerGainPerTick + (portal.ExtraPower * portal.WavesCompleted);
        portal.CurrentPower = Math.Min(portal.CurrentPower + gained, portal.MaxPower);

        // Increment wave counter for next tick
        portal.WavesCompleted++;

        TrySpawn(ent);
    }

    private void OnComponentShutdown(Entity<VoidPortalComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.PortalOwner == null)
            return;

        _summonPortal.PortalDestroyed(ent.Comp.PortalOwner.Value);
    }

    /// <summary>
    /// Spawns mobs until current power cannot afford anymore
    /// </summary>
    /// <param name="ent"></param> The void portal
    private void TrySpawn(Entity<VoidPortalComponent> ent)
    {
        var portal = ent.Comp;
        var mobs = portal.MobEntries.OrderByDescending(e => e.Cost).ToList();

        // Basically picks mobs to spawn at random until it does not have enough points to spawn any more mobs.
        while (portal.CurrentPower > 0)
        {
            var valid = mobs.Where(e => e.Cost <= portal.CurrentPower).ToList();
            if (valid.Count == 0)
                break;

            var chosen = _random.Pick(valid);

            portal.CurrentPower -= chosen.Cost;
            Spawn(chosen.Prototype, Transform(ent.Owner).Coordinates);
        }
    }
}
