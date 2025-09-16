using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Examine;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System;
using System.Linq;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class VoidPortalSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidPortalComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<VoidPortalComponent>();
        while (query.MoveNext(out var uid, out var portal))
        {
            UpdatePortal(uid, portal, frameTime);
        }
    }

    private void OnExamined(EntityUid uid, VoidPortalComponent? portal, ExaminedEvent args)
    {
        // Only show info to wraith
        if (!HasComp<WraithComponent>(args.Examiner))
            return;

        if (portal == null)
            return;

        // Display current portal power
        args.PushMarkup($"[color=mediumpurple]{Loc.GetString("void-portal-current-power", ("points", portal.CurrentPower))}[/color]");
        // Display current wave number
        args.PushMarkup($"[color=mediumpurple]{Loc.GetString("void-portal-current-wave", ("wave", portal.WavesCompleted))}[/color]");
    }

    private void UpdatePortal(EntityUid uid, VoidPortalComponent portal, float frameTime)
    {
        portal.accumulator += TimeSpan.FromSeconds(frameTime);

        if (portal.accumulator >= portal.SpawnInterval)
        {
            portal.accumulator -= portal.SpawnInterval;

            // --- Wave Power Growth ---
            // Each wave, portal gains PowerGainPerTick + (ExtraPower * number of waves so far).
            // This makes later waves stronger.
            var gained = portal.PowerGainPerTick + (portal.ExtraPower * portal.WavesCompleted);
            portal.CurrentPower += gained;

            // Increment wave counter for next tick
            portal.WavesCompleted++;

            // Can only hold up to 30 points, so enforce it here.
            if (portal.MaxPower > 0)
                portal.CurrentPower = Math.Min(portal.CurrentPower, portal.MaxPower);

            // Spawn mobs until CurrentPower cannot afford any entry
            TrySpawn(uid, portal);
        }
    }

    private void TrySpawn(EntityUid uid, VoidPortalComponent portal)
    {
        // Basically picks mobs to spawn at random until it does not have enough points to spawn any more mobs.
        while (true)
        {
            var valid = portal.MobEntries
                .Where(e => e.Cost <= portal.CurrentPower)
                .ToList();

            if (valid.Count == 0)
                break;

            var chosen = _random.Pick(valid);

            portal.CurrentPower -= chosen.Cost;
            Spawn(chosen.Prototype, Transform(uid).Coordinates);
        }
    }
}
