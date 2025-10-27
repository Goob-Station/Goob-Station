using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Shared.Mobs;

/// <summary>
/// This handles on compnent create it changes the mobstate thresholds
/// </summary>
public sealed class ChangeThresholdSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _mobThreshold= null!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangeThresholdComponent,ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ChangeThresholdComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentStartup(Entity<ChangeThresholdComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<MobThresholdsComponent>(ent.Owner, out var thresholds))
            return;
        ent.Comp.OldDeadThreshold = _mobThreshold.GetThresholdForState(ent.Owner, MobState.Dead);
        ent.Comp.OldCriticalThreshold = _mobThreshold.GetThresholdForState(ent.Owner, MobState.Critical);

        /// becuse problems can arise if there are two trigrs on the same value, death is changed first then critical
        _mobThreshold.SetMobStateThreshold(ent.Owner,
            (ent.Comp.OldDeadThreshold*ent.Comp.NewDeadMultiplyer),MobState.Dead);

        if (thresholds.Thresholds.ContainsValue(MobState.Critical)) // some species like IPC  dont have critical threshold
            _mobThreshold.SetMobStateThreshold(ent.Owner,
                (ent.Comp.OldCriticalThreshold * ent.Comp.NewCriticalMultiplyer),MobState.Critical);
    }
    private void OnComponentShutdown(Entity<ChangeThresholdComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<MobThresholdsComponent>(ent.Owner, out var thresholds))
            return;

        if (thresholds.Thresholds.ContainsValue(MobState.Critical))
            _mobThreshold.SetMobStateThreshold(ent.Owner, ent.Comp.OldCriticalThreshold,MobState.Critical);

        _mobThreshold.SetMobStateThreshold(ent.Owner, ent.Comp.OldDeadThreshold,MobState.Dead);
    }
}
