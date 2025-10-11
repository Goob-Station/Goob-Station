using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Shared.Mobs;

/// <summary>
/// This handles on compnent create it changes the mobstate thresholds
/// </summary>
public sealed class ChangeThresholdSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ChangeThresholdComponent,ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ChangeThresholdComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentStartup(Entity<ChangeThresholdComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<MobThresholdsComponent>(ent.Owner, out var thresholds))
            return;

        var critical = false;
        var death = false;
        foreach (var var in thresholds.Thresholds)
        {
            if (var.Value == MobState.Critical)
            {
                if (var.Key == ent.Comp.NewCriticalThreshold)
                    continue;

                ent.Comp.OldCriticalThreshold = var.Key;
                critical = true;
            }

            if (var.Value == MobState.Dead)
            {
                if (var.Key == ent.Comp.NewDeadThreshold)
                    continue;

                ent.Comp.OldDeadThreshold = var.Key;
                death = true;
            }
        }

        if (critical)
        {
            thresholds.Thresholds.Remove(ent.Comp.OldCriticalThreshold);
            thresholds.Thresholds.Add(ent.Comp.NewCriticalThreshold, MobState.Critical);
        }

        if (death)
        {
            thresholds.Thresholds.Remove(ent.Comp.OldDeadThreshold);
            thresholds.Thresholds.Add(ent.Comp.NewDeadThreshold, MobState.Dead);
        }
    }
    private void OnComponentShutdown(Entity<ChangeThresholdComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<MobThresholdsComponent>(ent.Owner, out var thresholds))
            return;

        var critical = false;
        var death = false;

        foreach (var var in thresholds.Thresholds)
        {
            if (var.Value == MobState.Critical)
            {
                if (var.Key == ent.Comp.OldCriticalThreshold)
                    continue;
                if (ent.Comp.NewCriticalThreshold == ent.Comp.OldCriticalThreshold)
                    continue;

                critical = true;
                //thresholds.Thresholds.Remove(var.Key);
                //thresholds.Thresholds.Add(ent.Comp.OldCriticalThreshold, var.Value);
            }

            if (var.Value == MobState.Dead)
            {
                if (var.Key == ent.Comp.OldDeadThreshold)
                    continue;
                if (ent.Comp.NewDeadThreshold == ent.Comp.OldDeadThreshold)
                    continue;

                death = true;
                //thresholds.Thresholds.Remove(var.Key);
                //thresholds.Thresholds.Add(ent.Comp.OldDeadThreshold, var.Value);
            }
        }

        if (critical)
        {
            thresholds.Thresholds.Remove(ent.Comp.NewCriticalThreshold);
            thresholds.Thresholds.Add(ent.Comp.OldCriticalThreshold, MobState.Critical);
        }

        if (death)
        {
            thresholds.Thresholds.Remove(ent.Comp.NewDeadThreshold);
            thresholds.Thresholds.Add(ent.Comp.OldDeadThreshold, MobState.Dead);
        }
    }
}
