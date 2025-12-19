using Content.Goobstation.Shared.Disease.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Disease.Systems;

public partial class SharedDiseaseSystem
{
    protected virtual void InitializeConditions()
    {
        SubscribeLocalEvent<DiseaseChanceConditionComponent, DiseaseCheckConditionsEvent>(CheckChanceCondition);
        SubscribeLocalEvent<DiseasePeriodicConditionComponent, DiseaseCheckConditionsEvent>(CheckPeriodicCondition);
        SubscribeLocalEvent<DiseaseProgressConditionComponent, DiseaseCheckConditionsEvent>(CheckSeverityCondition);
    }

    private void CheckChanceCondition(Entity<DiseaseChanceConditionComponent> ent, ref DiseaseCheckConditionsEvent args)
    {
        args.DoEffect = args.DoEffect && _random.Prob(ent.Comp.Chance * GetScale(args, ent.Comp));
    }

    private void CheckPeriodicCondition(Entity<DiseasePeriodicConditionComponent> ent, ref DiseaseCheckConditionsEvent args)
    {
        if (ent.Comp.CurrentDelay == null)
        {
            if (_net.IsClient)
            {
                args.DoEffect = false;
                return;
            }
            ent.Comp.CurrentDelay = TimeSpan.FromSeconds(_random.NextDouble(ent.Comp.DelayMin.TotalSeconds, ent.Comp.DelayMax.TotalSeconds));
        }

        ent.Comp.TimeSinceLast += TimeSpan.FromSeconds(GetScale(args, ent.Comp));
        if (ent.Comp.TimeSinceLast > ent.Comp.CurrentDelay)
        {
            ent.Comp.TimeSinceLast = TimeSpan.FromSeconds(0);
            ent.Comp.CurrentDelay = null;
            args.DoEffect = args.DoEffect && true;
        }
        else
            args.DoEffect = false;

        Dirty(ent);
    }

    private void CheckSeverityCondition(Entity<DiseaseProgressConditionComponent> ent, ref DiseaseCheckConditionsEvent args)
    {
        args.DoEffect = args.DoEffect
            && (ent.Comp.MinProgress == null || args.Disease.Comp.InfectionProgress > ent.Comp.MinProgress)
            && (ent.Comp.MaxProgress == null || args.Disease.Comp.InfectionProgress > ent.Comp.MaxProgress);
    }

    protected float GetScale(DiseaseCheckConditionsEvent args, ScalingDiseaseEffect effect)
    {
        return (effect.SeverityScale ? args.Comp.Severity : 1f)
            * (effect.TimeScale ? (float)_updateInterval.TotalSeconds : 1f)
            * (effect.ProgressScale ? args.Disease.Comp.InfectionProgress : 1f);
    }
}
