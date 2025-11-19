using Content.Shared._Gabystation.MalfAi;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared.Alert.Components;

namespace Content.Client._Gabystation.MalfAi;

public sealed class MalfAiSystem : SharedMalfAiSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfunctioningAiComponent, GetGenericAlertCounterAmountEvent>(OnAlertCounter);
    }

    private void OnAlertCounter(Entity<MalfunctioningAiComponent> malf, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Alert.ID != malf.Comp.CurrencyAlertId)
            return;

        args.Amount = (int) malf.Comp.CpuStore;
    }
}