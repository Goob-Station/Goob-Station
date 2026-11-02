using Content.Goobstation.Common.Carrying;
using Content.Goobstation.Common.Cuffs;
using Content.Goobstation.Common.Weapons;
using Content.Shared.Gravity;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModWearerEffectsSystem : EntitySystem
{
    [Dependency] private SharedGravitySystem _gravity = default!;

    [SubscribeLocalEvent]
    private void OnCarryDuration(Entity<ModQuickCarryComponent> ent, ref ModifyCarryDurationEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }

    [SubscribeLocalEvent]
    private void OnCuffDuration(Entity<ModQuickCuffComponent> ent, ref ModifyCuffDurationEvent args)
    {
        args.Duration *= ent.Comp.Multiplier;
    }

    [SubscribeLocalEvent]
    private void OnShoveAttempt(Entity<ModShoveBlockerComponent> ent, ref ShoveAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnAntiGravityStartup(Entity<ModAntiGravityComponent> ent, ref ComponentStartup args)
    {
        _gravity.RefreshWeightless(ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnAntiGravityShutdown(Entity<ModAntiGravityComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent))
            _gravity.RefreshWeightless(ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnIsWeightless(Entity<ModAntiGravityComponent> ent, ref IsWeightlessEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        args.IsWeightless = true;
        args.Handled = true;
    }
}
