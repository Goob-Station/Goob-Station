using Content.Server.Store.Systems;
using Content.Shared._Gabystation.MalfAi;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared.Alert;
using Content.Shared.Store.Components;

namespace Content.Server._Gabystation.MalfAi;

public sealed partial class MalfAiSystem : SharedMalfAiSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfunctioningAiComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<MalfunctioningAiComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<MalfunctioningAiComponent, CurrencyUpdatedEvent>(OnCurrencyUpdated);

        InitializeAbilities();
    }

    private void OnComponentInit(Entity<MalfunctioningAiComponent> malf, ref ComponentInit args)
    {
        _alerts.ShowAlert(malf.Owner, malf.Comp.CurrencyAlertId);
    }

    private void OnComponentShutdown(Entity<MalfunctioningAiComponent> malf, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(malf.Owner, malf.Comp.CurrencyAlertId);
    }

    private void OnCurrencyUpdated(Entity<MalfunctioningAiComponent> malf, ref CurrencyUpdatedEvent args)
    {
        if (!args.Currency.TryGetValue(malf.Comp.CurrencyId, out var amount))
            return;

        malf.Comp.CpuStore += amount;
        Dirty(malf);
    }

    public bool AddCpuPoints(Entity<MalfunctioningAiComponent?, StoreComponent?> malf, uint amount)
    {
        if (!Resolve(malf.Owner, ref malf.Comp1)
            || !Resolve(malf.Owner, ref malf.Comp2))
            return false;

        return _store.TryAddCurrency(new() { [malf.Comp1.CurrencyId] = amount }, malf);
    }
}
