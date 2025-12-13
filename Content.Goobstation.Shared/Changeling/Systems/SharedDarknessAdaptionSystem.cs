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
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    private EntityQuery<ChameleonSkinComponent> _chameleonQuery;
    private EntityQuery<StealthOnMoveComponent> _stealthOnMoveQuery;

    public override void Initialize()
    {
        base.Initialize();

        _chameleonQuery = GetEntityQuery<ChameleonSkinComponent>();
        _stealthOnMoveQuery = GetEntityQuery<StealthOnMoveComponent>();

        SubscribeLocalEvent<DarknessAdaptionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DarknessAdaptionComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<DarknessAdaptionComponent, ActionDarknessAdaptionEvent>(OnToggleAbility);
        SubscribeLocalEvent<DarknessAdaptionComponent, ChangelingChemicalRegenEvent>(OnChangelingChemicalRegenEvent);
    }

    private void OnMapInit(Entity<DarknessAdaptionComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        // so this doesnt mess over any other abilities or systems
        ent.Comp.HadLightDetection = HasComp<LightDetectionComponent>(ent);

        ent.Comp.ActionEnt = _actions.AddAction(ent, ent.Comp.ActionId);

        Dirty(ent);
    }

    private void OnShutdown(Entity<DarknessAdaptionComponent> ent, ref ComponentShutdown args)
    {
        HandleSpecialComponents(ent);
        HandleAlerts(ent, false);
        SetAdaptingBool(ent, false);

        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    #region Event Handlers

    private void OnToggleAbility(Entity<DarknessAdaptionComponent> ent, ref ActionDarknessAdaptionEvent args)
    {
        ent.Comp.Active = !ent.Comp.Active;
        DirtyField(ent, ent.Comp, nameof(DarknessAdaptionComponent.Active));

        var popup = ent.Comp.Active ? ent.Comp.ActivePopup : ent.Comp.InactivePopup;

        if (!ent.Comp.Active)
        {
            AdjustAdaption(ent, ent.Comp.Active);
            HandleSpecialComponents(ent);
            HandleAlerts(ent, ent.Comp.Active);
        }
        else
            EnsureComp<LightDetectionComponent>(ent);

        DoPopup(ent, popup);
    }

    private void OnChangelingChemicalRegenEvent(Entity<DarknessAdaptionComponent> ent, ref ChangelingChemicalRegenEvent args)
    {
        if (ent.Comp.Active
            && ent.Comp.Adapting)
            args.Modifier -= ent.Comp.ChemicalModifier;
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

    private void AdjustAdaption(Entity<DarknessAdaptionComponent> ent, bool adapting)
    {
        if (adapting)
        {
            EnsureAndSetStealth(ent);
            EnsureNightVision(ent);
        }
        else
        {
            RemComp<StealthComponent>(ent);
            RemComp<NightVisionComponent>(ent);
        }

        SetAdaptingBool(ent, adapting);
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

        DirtyField(ent, ent.Comp, nameof(DarknessAdaptionComponent.AlertDisplayed));
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

    private void SetAdaptingBool(Entity<DarknessAdaptionComponent> ent, bool adapting)
    {
        if (adapting)
            ent.Comp.Adapting = true;

        else if (!ent.Comp.Active
            || !adapting)
            ent.Comp.Adapting = false;

        DirtyField(ent, ent.Comp, nameof(DarknessAdaptionComponent.Adapting));
    }

    private void EnsureAndSetStealth(Entity<DarknessAdaptionComponent> ent)
    {
        if (_chameleonQuery.TryComp(ent, out var chameleon)) // so it doesnt interfere
            chameleon.Active = false;

        EnsureComp<StealthComponent>(ent, out var stealth);

        stealth.RevealOnAttack = false;
        stealth.RevealOnDamage = false;

        _stealth.SetEnabled(ent, true, stealth);
        _stealth.SetVisibility(ent, ent.Comp.Visibility, stealth);

        if (_stealthOnMoveQuery.HasComp(ent))
            RemCompDeferred<StealthOnMoveComponent>(ent);
    }

    private void EnsureNightVision(Entity<DarknessAdaptionComponent> ent)
    {
        var nightVision = Factory.GetComponent<NightVisionComponent>();
        nightVision.IsActive = true;
        nightVision.Color = Color.FromHex("#606cb3");
        nightVision.ActivateSound = null;
        nightVision.DeactivateSound = null;
        nightVision.ToggleAction = null;

        AddComp(ent, nightVision, true);
    }

    private void HandleSpecialComponents(Entity<DarknessAdaptionComponent> ent)
    {
        RemComp<NightVisionComponent>(ent);

        if (!ent.Comp.HadLightDetection)
            RemComp<LightDetectionComponent>(ent); // saves on performance if the ability isn't active

        RemComp<StealthComponent>(ent);
        RemComp<StealthOnMoveComponent>(ent);
    }
    #endregion
}
