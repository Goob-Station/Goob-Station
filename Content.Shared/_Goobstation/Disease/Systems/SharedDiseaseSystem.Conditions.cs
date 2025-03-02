using Robust.Shared.Network;
using Robust.Shared.Random;
using System;

namespace Content.Shared.Disease;

public partial class SharedDiseaseSystem
{
    [Dependency] private readonly INetManager _net = default!;

    protected virtual void InitializeConditions()
    {
        SubscribeLocalEvent<DiseasePeriodicConditionComponent, DiseaseCheckConditionsEvent>(CheckPeriodicCondition);
        SubscribeLocalEvent<DiseaseProgressConditionComponent, DiseaseCheckConditionsEvent>(CheckSeverityCondition);
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
            && (condition.MinProgress == null || args.DiseaseProgress > condition.MinProgress)
            && (condition.MaxProgress == null || args.DiseaseProgress > condition.MaxProgress);
    }

    protected float GetScale(DiseaseCheckConditionsEvent args, ScalingDiseaseEffect effect)
    {
        return (effect.SeverityScale ? args.Severity : 1f)
            * (effect.TimeScale ? (float)args.TimeDelta.TotalSeconds : 1f)
            * (effect.ProgressScale ? args.DiseaseProgress : 1f);
    }
}
