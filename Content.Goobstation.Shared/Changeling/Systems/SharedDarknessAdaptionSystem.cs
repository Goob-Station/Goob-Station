using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract class SharedDarknessAdaptionSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;
    private EntityQuery<ChameleonSkinComponent> _chameleonQuery;
    private EntityQuery<NightVisionComponent> _nvgQuery;
    private EntityQuery<StealthOnMoveComponent> _stealthOnMoveQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
        _chameleonQuery = GetEntityQuery<ChameleonSkinComponent>();
        _nvgQuery = GetEntityQuery<NightVisionComponent>();
        _stealthOnMoveQuery = GetEntityQuery<StealthOnMoveComponent>();

        SubscribeLocalEvent<DarknessAdaptionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DarknessAdaptionComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<DarknessAdaptionComponent, ActionDarknessAdaptionEvent>(OnToggleAbility);
    }

    private void OnMapInit(Entity<DarknessAdaptionComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        // so this doesnt mess over any other abilities or systems
        ent.Comp.HadLightDetection = HasComp<LightDetectionComponent>(ent);

        EnsureComp<LightDetectionComponent>(ent);

        var nightVision = _compFactory.GetComponent<NightVisionComponent>();
        nightVision.IsActive = false;
        nightVision.Color = Color.FromHex("#565b7a");
        nightVision.ActivateSound = null;
        nightVision.DeactivateSound = null;
        nightVision.ToggleAction = null;

        AddComp(ent, nightVision, true);

        ent.Comp.ActionEnt = _actions.AddAction(ent, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<DarknessAdaptionComponent> ent, ref ComponentShutdown args)
    {
        HandleSpecialComponents(ent);
        HandleAlerts(ent, false);
        SetChemicalModifier(ent, false);

        RemComp<NightVisionComponent>(ent);

        if (!ent.Comp.HadLightDetection)
            RemComp<LightDetectionComponent>(ent);

        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    #region Event Handlers

    private void OnToggleAbility(Entity<DarknessAdaptionComponent> ent, ref ActionDarknessAdaptionEvent args)
    {
        ent.Comp.Active = !ent.Comp.Active;
        var popup = ent.Comp.Active ? ent.Comp.ActivePopup : ent.Comp.InactivePopup;

        if (!ent.Comp.Active)
        {
            HandleSpecialComponents(ent);
            HandleAlerts(ent, ent.Comp.Active);
            SetChemicalModifier(ent, ent.Comp.Active);
        }

        DoPopup(ent, popup);
    }
    #endregion

    #region Helper Methods
    protected void DoAbility(Entity<DarknessAdaptionComponent> ent, bool state)
    {
        if (FireInvalidCheck(ent))
        {
            AdjustAdaption(ent, false);
            HandleAlerts(ent, false);
            return;
        }

        AdjustAdaption(ent, state);
        HandleAlerts(ent, state);
    }

    private void AdjustAdaption(Entity<DarknessAdaptionComponent> ent, bool state)
    {
        EnsureAndSetStealth(ent, state);
        SetChemicalModifier(ent, state);

        if (!_nvgQuery.TryComp(ent, out var nvg))
            return;

        nvg.IsActive = state;
        Dirty(ent, nvg);
    }

    private void HandleAlerts(Entity<DarknessAdaptionComponent> ent, bool show)
    {
        if (show && !ent.Comp.AlertDisplayed)
        {
            _alerts.ShowAlert(
                ent,
                ent.Comp.AlertId);

            ent.Comp.AlertDisplayed = true;
        }

        if (!show && ent.Comp.AlertDisplayed)
        {
            _alerts.ClearAlert(
                ent,
                ent.Comp.AlertId);

            ent.Comp.AlertDisplayed = false;
        }
    }

    private bool FireInvalidCheck(Entity<DarknessAdaptionComponent> ent)
    {
        var fireEv = new GetFireStateEvent();
        RaiseLocalEvent(ent, ref fireEv);

        return fireEv.OnFire;
    }

    private void DoPopup(Entity<DarknessAdaptionComponent> ent, LocId popup)
    {
        if (_net.IsClient)
            return;

        _popup.PopupEntity(Loc.GetString(popup), ent, ent);
    }

    private void SetChemicalModifier(Entity<DarknessAdaptionComponent> ent, bool adapting)
    {
        if (!_lingQuery.TryComp(ent, out var ling))
            return;

        if (!ent.Comp.ModifierApplied // adapting
            && adapting)
        {
            ent.Comp.ModifierApplied = true;
            ling.ChemicalRegenMultiplier -= ent.Comp.ChemicalModifier;
        }

        if (ent.Comp.ModifierApplied // not adapting
            && !adapting)
        {
            ent.Comp.ModifierApplied = false;
            ling.ChemicalRegenMultiplier += ent.Comp.ChemicalModifier;
        }

        if (!ent.Comp.Active // disabled
            && ent.Comp.ModifierApplied)
        {
            ent.Comp.ModifierApplied = false;
            ling.ChemicalRegenMultiplier += ent.Comp.ChemicalModifier;
        }

        Dirty(ent, ling);
    }

    private void EnsureAndSetStealth(Entity<DarknessAdaptionComponent> ent, bool state)
    {
        if (_chameleonQuery.TryComp(ent, out var chameleon)) // so it doesnt interfere
            chameleon.Active = false;

        EnsureComp<StealthComponent>(ent, out var stealth);

        _stealth.SetEnabled(ent, state, stealth);
        _stealth.SetVisibility(ent, ent.Comp.Visibility, stealth);
        stealth.RevealOnAttack = false;
        stealth.RevealOnDamage = false;

        if (_stealthOnMoveQuery.HasComp(ent))
            RemCompDeferred<StealthOnMoveComponent>(ent);

        Dirty(ent, stealth);
    }

    private void HandleSpecialComponents(Entity<DarknessAdaptionComponent> ent)
    {
        if (_nvgQuery.TryComp(ent, out var nvg))
        {
            nvg.IsActive = false;
            Dirty(ent, nvg);
        }

        RemComp<StealthComponent>(ent);
        RemComp<StealthOnMoveComponent>(ent);
    }
    #endregion
}
