using Robust.Shared.Network;
using Robust.Shared.Random;
using System;

namespace Content.Shared.Disease;

public partial class SharedDiseaseSystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected virtual void InitializeConditions()
    {
        SubscribeLocalEvent<DiseasePeriodicConditionComponent, DiseaseCheckConditionsEvent>(CheckPeriodicCondition);
    }

    private void CheckPeriodicCondition(EntityUid uid, DiseasePeriodicConditionComponent condition, DiseaseCheckConditionsEvent args)
    {
        if (condition.CurrentDelay == null)
        {
            if (_net.IsClient)
                return;
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

    protected float GetScale(DiseaseCheckConditionsEvent args, BaseDiseaseEffect condition)
    {
        return (condition.SeverityScale ? args.Severity : 1f) * (condition.TimeScale ? (float)args.TimeDelta.TotalSeconds : 1f);
    }
}
