using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared.Alert;
using Content.Shared.Atmos.Components;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract partial class SharedChangelingChemicalSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();

        SubscribeLocalEvent<ChangelingChemicalComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChangelingChemicalComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ChangelingChemicalComponent, ChangelingModifyChemicalsEvent>(OnModifyChemicalsEvent);
        SubscribeLocalEvent<ChangelingChemicalComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnMapInit(Entity<ChangelingChemicalComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;
        DirtyField(ent, ent.Comp, nameof(ChangelingChemicalComponent.UpdateTimer));

        Cycle(ent);
    }

    private void OnShutdown(Entity<ChangelingChemicalComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent, ent.Comp.AlertId);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChangelingChemicalComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;
            Dirty(uid, comp);

            Cycle((uid, comp));
        }
    }

    private void Cycle(Entity<ChangelingChemicalComponent> ent)
    {
        RegenerateChemicals(ent);
    }

    #region Helper Methods
    private void RegenerateChemicals(Entity<ChangelingChemicalComponent> ent)
    {
        var regenEv = new ChangelingChemicalRegenEvent(ent.Comp.Modifier);
        RaiseLocalEvent(ent, ref regenEv);

        var baseMult = regenEv.Modifier;
        var fireMult = OnFire(ent) ? ent.Comp.FireModifier : 1f;

        var amount = ent.Comp.RegenAmount * baseMult * fireMult;

        UpdateChemicals(ent, amount);
    }

    private void UpdateChemicals(Entity<ChangelingChemicalComponent> ent, float? amount = null)
    {
        var chemicals = ent.Comp.Chemicals;

        chemicals += amount ?? 0;
        ent.Comp.Chemicals = Math.Clamp(chemicals, 0, ent.Comp.MaxChemicals);

        Dirty(ent);

        _alerts.ShowAlert(ent, ent.Comp.AlertId);
    }

    private bool OnFire(Entity<ChangelingChemicalComponent> ent)
    {
        return HasComp<OnFireComponent>(ent);
    }

    #endregion

    #region Event Handlers
    private void OnModifyChemicalsEvent(Entity<ChangelingChemicalComponent> ent, ref ChangelingModifyChemicalsEvent args)
    {
        UpdateChemicals(ent, args.Amount);
    }

    private void OnRejuvenate(Entity<ChangelingChemicalComponent> ent, ref RejuvenateEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInStasis)
            return;

        UpdateChemicals(ent, ent.Comp.MaxChemicals);
        _popup.PopupEntity(Loc.GetString("changeling-rejuvenate"), ent, ent);
    }

    #endregion
}
