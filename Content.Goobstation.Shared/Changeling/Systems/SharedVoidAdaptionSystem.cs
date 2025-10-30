using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Temperature;
using Content.Goobstation.Shared.Atmos.Events;
using Content.Goobstation.Shared.Body;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Temperature;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed class SharedVoidAdaptionSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();

        SubscribeLocalEvent<VoidAdaptionComponent, ComponentRemove>(OnRemoved);

        SubscribeLocalEvent<VoidAdaptionComponent, ResistPressureEvent>(OnGetDangerousPressure);
        SubscribeLocalEvent<VoidAdaptionComponent, SendSafePressureEvent>(OnGetSafePressure);
        SubscribeLocalEvent<VoidAdaptionComponent, BeforeTemperatureChange>(BeforeTemperatureChangeAttempt);
        SubscribeLocalEvent<VoidAdaptionComponent, TemperatureImmunityEvent>(OnTemperatureImmunityCheck);
        SubscribeLocalEvent<VoidAdaptionComponent, CheckNeedsAirEvent>(OnCheckNeedsAir);
    }

    private void OnRemoved(Entity<VoidAdaptionComponent> ent, ref ComponentRemove args)
    {
        // incase something removes the component
        _alerts.ClearAlert(
            ent,
            ent.Comp.Alert);

        if (!_lingQuery.TryComp(ent, out var ling)
            || !ent.Comp.AdaptingLowPressure
            && !ent.Comp.AdaptingLowTemp)
            return;

        ling.ChemicalRegenMultiplier += ent.Comp.ChemModifierValue;
        Dirty(ent, ling);

    }

    #region Event Handlers

    public readonly float LowP = Atmospherics.HazardLowPressure;
    private void OnGetDangerousPressure(Entity<VoidAdaptionComponent> ent, ref ResistPressureEvent args)
    {
        if (!FireValidCheck(ent))
            return;

        if (args.Pressure >= LowP)
            return;

        if (!ent.Comp.AdaptingLowPressure)
        {
            DoSituationPopup(ent, ent.Comp.EnterLowPressurePopup);
            TryApplyDebuff(ent);
            ent.Comp.AdaptingLowPressure = true;
        }

        args.Cancelled = true;

    }

    private void OnGetSafePressure(Entity<VoidAdaptionComponent> ent, ref SendSafePressureEvent args)
    {
        if (!FireValidCheck(ent)) // this ends up doing the "same" thing, but it stops the popup that tells the ling they're safe
            return;

        if (!ent.Comp.AdaptingLowPressure)
            return;

        ent.Comp.AdaptingLowPressure = false;
        DoSituationPopup(ent, ent.Comp.LeaveLowPressurePopup);
        TryRemoveDebuff(ent);

    }

    private void BeforeTemperatureChangeAttempt(Entity<VoidAdaptionComponent> ent, ref BeforeTemperatureChange args)
    {
        if (!FireValidCheck(ent))
            return;

        var compareT = GetTempThreshold(ent);
        var safeT = compareT + 1f;

        var newTemp = args.CurrentTemperature;
        var lastTemp = args.LastTemperature;
        var diff = args.TemperatureDelta;

        if (newTemp <= compareT
            && !ent.Comp.AdaptingLowTemp)
        {
            DoSituationPopup(ent, ent.Comp.EnterLowTempPopup);
            TryApplyDebuff(ent);
            ent.Comp.AdaptingLowTemp = true;
        }
        else if (newTemp > compareT
            && lastTemp > safeT + diff
            && ent.Comp.AdaptingLowTemp)
        {
            ent.Comp.AdaptingLowTemp = false;
            DoSituationPopup(ent, ent.Comp.LeaveLowTempPopup);
            TryRemoveDebuff(ent);
        }
    }

    private void OnTemperatureImmunityCheck(Entity<VoidAdaptionComponent> ent, ref TemperatureImmunityEvent args)
    {
        if (!FireValidCheck(ent))
            return;

        var safeThreshold = GetTempThreshold(ent) + 1;

        if (args.CurrentTemperature < safeThreshold)
            args.CurrentTemperature = safeThreshold;
    }

    private void OnCheckNeedsAir(Entity<VoidAdaptionComponent> ent, ref CheckNeedsAirEvent args)
    {
        if (!FireValidCheck(ent))
            return;

        args.Cancelled = true;
    }

    #endregion

    #region Helper Methods

    public readonly ProtoId<AlertPrototype> LowPressureAlert = "LowPressure";
    public readonly ProtoId<AlertPrototype> LowTempAlert = "Cold";
    private bool FireValidCheck(Entity<VoidAdaptionComponent> ent)
    {
        if (!OnFire(ent))
        {
            ent.Comp.FirePopupSent = false;

            // void adaption recovering from fire can have these alerts show up (mainly when in space/vacuum)
            _alerts.ClearAlert(
            ent,
            LowPressureAlert);

            _alerts.ClearAlert(
            ent,
            LowTempAlert);

            return true;
        }

        if (ent.Comp.AdaptingLowPressure
            || ent.Comp.AdaptingLowTemp
            && !ent.Comp.FirePopupSent)
        {
            DoSituationPopup(ent, ent.Comp.FirePopup);
            ent.Comp.FirePopupSent = true;
        }

        ent.Comp.AdaptingLowPressure = false;
        ent.Comp.AdaptingLowTemp = false;

        TryRemoveDebuff(ent);

        return false;
    }

    private bool OnFire(Entity<VoidAdaptionComponent> ent)
    {
        var fireEv = new GetFireStateEvent();
        RaiseLocalEvent(ent, ref fireEv);

        return fireEv.OnFire;
    }

    private float GetTempThreshold(Entity<VoidAdaptionComponent> ent)
    {
        var thresholdEv = new GetTemperatureThresholdsEvent();
        RaiseLocalEvent(ent, ref thresholdEv);

        var freezeT = thresholdEv.ColdDamageThreshold;

        var highestSpeedT = thresholdEv.SpeedThresholds != null
            ? thresholdEv.SpeedThresholds.Keys.Max()
            : freezeT; // only if TemperatureSpeedComponent doesnt exist

        return Math.Max(freezeT, highestSpeedT);
    }

    private void TryApplyDebuff(Entity<VoidAdaptionComponent> ent)
    {
        if (!_lingQuery.TryComp(ent, out var ling))
            return;

        if (ent.Comp.AdaptingLowPressure
            || ent.Comp.AdaptingLowTemp)
            return;

        ling.ChemicalRegenMultiplier -= ent.Comp.ChemModifierValue;
        Dirty(ent, ling);

        _alerts.ShowAlert(
            ent,
            ent.Comp.Alert);
    }

    private void TryRemoveDebuff(Entity<VoidAdaptionComponent> ent)
    {
        if (!_lingQuery.TryComp(ent, out var ling))
            return;

        if (ent.Comp.AdaptingLowPressure
            || ent.Comp.AdaptingLowTemp)
            return;

        ling.ChemicalRegenMultiplier += ent.Comp.ChemModifierValue;
        Dirty(ent, ling);

        _alerts.ClearAlert(
            ent,
            ent.Comp.Alert);
    }

    private void DoSituationPopup(Entity<VoidAdaptionComponent> ent, LocId id)
    {
        if (_netManager.IsClient)
            return;

        _popup.PopupEntity(Loc.GetString(id), ent, ent);
    }
    #endregion
}
