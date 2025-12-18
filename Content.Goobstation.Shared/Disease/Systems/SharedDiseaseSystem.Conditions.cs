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

    private void CheckChanceCondition(EntityUid uid, DiseaseChanceConditionComponent condition, DiseaseCheckConditionsEvent args)
    {
        args.DoEffect = args.DoEffect && _random.Prob(condition.Chance * GetScale(args, condition));
    }

    private void CheckPeriodicCondition(EntityUid uid, DiseasePeriodicConditionComponent condition, DiseaseCheckConditionsEvent args)
    {
        if (condition.CurrentDelay == null)
        {
            if (_net.IsClient)
            {
                args.DoEffect = false;
                return;
            }
            condition.CurrentDelay = TimeSpan.FromSeconds(_random.NextDouble(condition.DelayMin.TotalSeconds, condition.DelayMax.TotalSeconds));
        }

        condition.TimeSinceLast += TimeSpan.FromSeconds(GetScale(args, condition));
        if (condition.TimeSinceLast > condition.CurrentDelay)
        {
            condition.TimeSinceLast = TimeSpan.FromSeconds(0);
            condition.CurrentDelay = null;
            args.DoEffect = args.DoEffect && true;
        }
        else
            args.DoEffect = false;

        Dirty(uid, condition);
    }

    private void CheckSeverityCondition(EntityUid uid, DiseaseProgressConditionComponent condition, DiseaseCheckConditionsEvent args)
    {
        args.DoEffect = args.DoEffect
            && (condition.MinProgress == null || args.Disease.Comp.InfectionProgress > condition.MinProgress)
            && (condition.MaxProgress == null || args.Disease.Comp.InfectionProgress > condition.MaxProgress);
    }

    protected float GetScale(DiseaseCheckConditionsEvent args, ScalingDiseaseEffect effect)
    {
        return (effect.SeverityScale ? args.Comp.Severity : 1f)
            * (effect.TimeScale ? (float)_updateInterval.TotalSeconds : 1f)
            * (effect.ProgressScale ? args.Disease.Comp.InfectionProgress : 1f);
    }
}
