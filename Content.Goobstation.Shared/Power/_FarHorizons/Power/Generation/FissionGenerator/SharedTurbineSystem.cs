using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Electrocution;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Repairable;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/turbine.dm

public abstract class SharedTurbineSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    private readonly float _threshold = 0.5f;
    private float _accumulator = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TurbineComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<TurbineComponent, TurbineChangeFlowRateMessage>(OnTurbineFlowRateChanged);
        SubscribeLocalEvent<TurbineComponent, TurbineChangeStatorLoadMessage>(OnTurbineStatorLoadChanged);

        SubscribeLocalEvent<TurbineComponent, InteractUsingEvent>(RepairTurbine);
        SubscribeLocalEvent<TurbineComponent, RepairFinishedEvent>(OnRepairTurbineFinished);
    }

    private void OnExamined(Entity<TurbineComponent> ent, ref ExaminedEvent args)
    {
        var comp = ent.Comp;
        if (!Comp<TransformComponent>(ent).Anchored || !args.IsInDetailsRange) // Not anchored? Out of range? No status.
            return;

        using (args.PushGroup(nameof(TurbineComponent)))
        {
            if (!comp.Ruined)
            {
                switch (comp.RPM)
                {
                    case float n when n is >= 0 and <= 1:
                        args.PushMarkup(Loc.GetString("turbine-spinning-0")); // " The blades are not spinning."
                        break;
                    case float n when n is > 1 and <= 60:
                        args.PushMarkup(Loc.GetString("turbine-spinning-1")); // " The blades are turning slowly."
                        break;
                    case float n when n > 60 && n <= comp.BestRPM * 0.5:
                        args.PushMarkup(Loc.GetString("turbine-spinning-2")); // " The blades are spinning."
                        break;
                    case float n when n > comp.BestRPM * 0.5 && n <= comp.BestRPM * 1.2:
                        args.PushMarkup(Loc.GetString("turbine-spinning-3")); // " The blades are spinning quickly."
                        break;
                    case float n when n > comp.BestRPM * 1.2 && n <= float.PositiveInfinity:
                        args.PushMarkup(Loc.GetString("turbine-spinning-4")); // " The blades are spinning out of control!"
                        break;
                    default:
                        break;
                }
            }

            if (comp.Ruined)
            {
                args.PushMarkup(Loc.GetString("turbine-ruined")); // " It's completely broken!"
            }
            else if (comp.BladeHealth <= 0.25 * comp.BladeHealthMax)
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-3")); // " It's critically damaged!"
            }
            else if (comp.BladeHealth <= 0.5 * comp.BladeHealthMax)
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-2")); // " The turbine looks badly damaged."
            }
            else if (comp.BladeHealth <= 0.75 * comp.BladeHealthMax)
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-1")); // " The turbine looks a bit scuffed."
            }
            else
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-0")); // " It appears to be in good condition."
            }
        }
    }

    public override void Update(float frameTime)
    {
        _accumulator += frameTime;
        if (_accumulator > _threshold)
        {
            AccUpdate();
            _accumulator = 0;
        }
    }

    protected virtual void AccUpdate() { }

    protected void UpdateAppearance(EntityUid uid, TurbineComponent? comp = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref comp, ref appearance, false))
            return;

        _appearance.TryGetData<bool>(uid, TurbineVisuals.TurbineRuined, out var IsSpriteRuined);
        if (comp.Ruined)
        {
            if (!IsSpriteRuined)
            {
                _appearance.SetData(uid, TurbineVisuals.TurbineRuined, true);
            }
        }
        else
        {
            if (IsSpriteRuined)
            {
                _appearance.SetData(uid, TurbineVisuals.TurbineRuined, false);
            }
            _appearance.SetData(uid, TurbineVisuals.TurbineSpeed, comp.RPM > 1);
        }

        _appearance.SetData(uid, TurbineVisuals.DamageSpark, comp.IsSparking);
        _appearance.SetData(uid, TurbineVisuals.DamageSmoke, comp.IsSmoking);
    }

    protected void PlayAudio(SoundSpecifier? sound, EntityUid uid, out EntityUid? audioStream, AudioParams? audioParams = null)
    {
        if (sound == null || audioParams == null)
        {
            audioStream = null;
            return;
        }

        var loop = audioParams.Value.WithLoop(true);
        var stream = false
            ? _audio.PlayPredicted(sound, uid, uid, loop)
            : _audio.PlayPvs(sound, uid, loop);
        audioStream = stream?.Entity is { } entity ? entity : null;
    }

    protected static bool AdjustStatorLoad(TurbineComponent turbine, float change)
    { 
        var newSet = Math.Clamp(turbine.StatorLoad + change, 1000f, turbine.StatorLoadMax);
        if (turbine.StatorLoad != newSet)
        {
            turbine.StatorLoad = newSet;
            return true;
        }
        return false; 
    }

    #region User Interface
    private void OnTurbineFlowRateChanged(EntityUid uid, TurbineComponent turbine, TurbineChangeFlowRateMessage args)
    {
        turbine.FlowRate = Math.Clamp(args.FlowRate, 0f, turbine.FlowRateMax);
        Dirty(uid, turbine);
        UpdateUI(uid, turbine);
        _adminLogger.Add(LogType.AtmosVolumeChanged, LogImpact.Medium,
            $"{ToPrettyString(args.Actor):player} set the flow rate on {ToPrettyString(uid):device} to {args.FlowRate}");
    }

    private void OnTurbineStatorLoadChanged(EntityUid uid, TurbineComponent turbine, TurbineChangeStatorLoadMessage args)
    {
        turbine.StatorLoad = Math.Clamp(args.StatorLoad, 1000f, turbine.StatorLoadMax);
        Dirty(uid, turbine);
        UpdateUI(uid, turbine);
        _adminLogger.Add(LogType.AtmosDeviceSetting, LogImpact.Medium,
            $"{ToPrettyString(args.Actor):player} set the stator load on {ToPrettyString(uid):device} to {args.StatorLoad}");
    }

    protected virtual void UpdateUI(EntityUid uid, TurbineComponent turbine) { }
    #endregion

    #region Repairs
    private void RepairTurbine(EntityUid uid, TurbineComponent comp, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (comp.BladeHealth >= comp.BladeHealthMax && !comp.Ruined)
            return;

        args.Handled = _toolSystem.UseTool(args.Used, args.User, uid, comp.RepairDelay, comp.RepairTool, new RepairFinishedEvent(), comp.RepairFuelCost);
    }

    //Gotta love server/client desync
    protected virtual void OnRepairTurbineFinished(Entity<TurbineComponent> ent, ref RepairFinishedEvent args)
    {
        if (args.Cancelled)
            return;

        if (!TryComp(ent.Owner, out TurbineComponent? comp))
            return;

        if (comp.Ruined)
        {
            comp.Ruined = false;
            if (comp.BladeHealth <= 0) { comp.BladeHealth = 1; }
            UpdateHealthIndicators(ent.Owner, comp);
            return;
        }
        else if (comp.BladeHealth < comp.BladeHealthMax)
        {
            comp.BladeHealth++;
            UpdateHealthIndicators(ent.Owner, comp);
            return;
        }
        else if (comp.BladeHealth >= comp.BladeHealthMax)
        {
            // This should technically never occur, but just in case...
            return;
        }
    }

    protected void UpdateHealthIndicators(EntityUid uid, TurbineComponent comp)
    {
        if (comp.BladeHealth <= 0.75 * comp.BladeHealthMax && !comp.IsSparking)
        {
            comp.IsSparking = true;
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/PowerSink/electric.ogg"), uid, AudioParams.Default.WithPitchScale(0.75f));
            _popupSystem.PopupEntity(Loc.GetString("turbine-spark", ("owner", uid)), uid, PopupType.MediumCaution);
        }
        else if (comp.BladeHealth > 0.75 * comp.BladeHealthMax && comp.IsSparking)
        {
            comp.IsSparking = false;
            _popupSystem.PopupEntity(Loc.GetString("turbine-spark-stop", ("owner", uid)), uid, PopupType.Medium);
        }

        if (comp.BladeHealth <= 0.5 * comp.BladeHealthMax && !comp.IsSmoking)
        {
            comp.IsSmoking = true;
            _popupSystem.PopupEntity(Loc.GetString("turbine-smoke", ("owner", uid)), uid, PopupType.MediumCaution);
        }
        else if (comp.BladeHealth > 0.5 * comp.BladeHealthMax && comp.IsSmoking)
        {
            comp.IsSmoking = false;
            _popupSystem.PopupEntity(Loc.GetString("turbine-smoke-stop", ("owner", uid)), uid, PopupType.Medium);
        }

        _entityManager.EnsureComponent<ElectrifiedComponent>(uid).Enabled = comp.IsSparking;

        UpdateAppearance(uid, comp);
    }

    #endregion
}
