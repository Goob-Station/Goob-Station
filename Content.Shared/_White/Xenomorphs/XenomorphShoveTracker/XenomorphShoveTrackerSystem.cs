using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.CombatMode;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Shared._White.Xenomorphs.XenomorphShoveTracker;

public sealed class XenomorphShoveTrackerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobStateComponent, DisarmedEvent>(OnDisarmed);
    }

    private void OnDisarmed(EntityUid uid, MobStateComponent mobStateComponent, ref DisarmedEvent args)
    {
        var source = args.Source;
        var target = args.Target;

        // Only process if the source is a xenomorph with shove tracker
        if (!HasComp<XenomorphComponent>(source) || !TryComp<XenomorphShoveTrackerComponent>(source, out var xenoComponent))
            return;

        var currentTime = _timing.CurTime;

        // Clean up old shove counts
        CleanupOldShoves(xenoComponent, currentTime);

        // Initialize or increment shove count
        if (!xenoComponent.ShoveCount.ContainsKey(target))
            xenoComponent.ShoveCount[target] = 0;

        xenoComponent.ShoveCount[target]++;
        xenoComponent.LastShoveTime[target] = currentTime;

        // Check if threshold reached
        if (xenoComponent.ShoveCount[target] >= xenoComponent.ShoveThreshold)
        {
            // Apply knockdown and stun
            bool wasStunned = _stun.TryStun(target, xenoComponent.KnockdownDuration, true);
            bool wasKnockedDown = _stun.TryKnockdown(target, xenoComponent.KnockdownDuration, true);

            // Reset the count for this target
            xenoComponent.ShoveCount.Remove(target);
            xenoComponent.LastShoveTime.Remove(target);
        }

        // Always handle xenomorph shoves to prevent normal disarm effects
        args.Handled = true;

        Dirty(source, xenoComponent);
    }

    private static void CleanupOldShoves(XenomorphShoveTrackerComponent component, TimeSpan currentTime)
    {
        var toRemove = new List<EntityUid>();

        foreach (var (target, lastTime) in component.LastShoveTime)
        {
            if (currentTime - lastTime > component.ShoveResetTime)
            {
                toRemove.Add(target);
            }
        }

        foreach (var target in toRemove)
        {
            component.ShoveCount.Remove(target);
            component.LastShoveTime.Remove(target);
        }
    }
}
