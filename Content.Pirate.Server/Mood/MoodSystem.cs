using System.Threading;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Shared.Alert;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Mood;
using Content.Shared.Overlays;
using Content.Shared.Popups;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Pirate.Server.Mood;

public sealed class MoodSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MoodComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<MoodComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MoodComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MoodComponent, MoodEffectEvent>(OnMoodEffect);
        SubscribeLocalEvent<MoodComponent, DamageChangedEvent>(OnDamageChange);
        SubscribeLocalEvent<MoodComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<MoodComponent, MoodRemoveEffectEvent>(OnRemoveEffect);
        SubscribeLocalEvent<MoodComponent, ShowMoodAlertEvent>(OnShowMoodAlert);
    }

    private void OnShowMoodAlert(EntityUid uid, MoodComponent component, ShowMoodAlertEvent args)
    {
        if (!_playerManager.TryGetSessionByEntity(uid, out var session))
            return;

        var msg = $"{Loc.GetString("mood-show-effects-start")}\n";

        foreach (var (_, protoId) in component.CategorisedEffects)
        {
            if (!_prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var proto) || proto.Hidden)
                continue;

            var color = proto.MoodChange > 0 ? "#008000" : "#BA0000";
            msg += $"[font size=10][color={color}]{proto.Description}[/color][/font]\n";
        }

        foreach (var (protoId, _) in component.UncategorisedEffects)
        {
            if (!_prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var proto) || proto.Hidden)
                continue;

            var color = proto.MoodChange > 0 ? "#008000" : "#BA0000";
            msg += $"[font size=10][color={color}]{proto.Description}[/color][/font]\n";
        }

        _chatManager.ChatMessageToOne(
            ChatChannel.Emotes,
            msg,
            msg,
            EntityUid.Invalid,
            false,
            session.Channel);
    }

    private void OnShutdown(EntityUid uid, MoodComponent component, ComponentShutdown args)
    {
        _alerts.ClearAlertCategory(uid, component.MoodCategory);
        RemComp<SaturationScaleOverlayComponent>(uid);

        foreach (var token in component.EffectTimeoutSources.Values)
            DisposeEffectTimeout(token);

        component.EffectTimeoutSources.Clear();
    }

    private void OnRemoveEffect(EntityUid uid, MoodComponent component, MoodRemoveEffectEvent args)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled))
            return;

        if (component.UncategorisedEffects.TryGetValue(args.EffectId, out _))
            RemoveTimedOutEffect(uid, args.EffectId);
        else
        {
            foreach (var (category, id) in component.CategorisedEffects)
            {
                if (id != args.EffectId)
                    continue;

                RemoveTimedOutEffect(uid, args.EffectId, category);
                return;
            }
        }
    }

    private void OnRefreshMoveSpeed(EntityUid uid, MoodComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled)
            || component.CurrentMoodThreshold is > MoodThreshold.Meh and < MoodThreshold.Good or MoodThreshold.Dead
            || _jetpack.IsUserFlying(uid))
            return;

        var modifier = Math.Clamp(
            component.CurrentMoodLevel >= component.MoodThresholds[MoodThreshold.Neutral]
                ? _config.GetCVar(CCVars.MoodIncreasesSpeed)
                    ? MathF.Pow(component.SpeedBonusGrowth, component.CurrentMoodLevel - component.MoodThresholds[MoodThreshold.Neutral])
                    : 1
                : _config.GetCVar(CCVars.MoodDecreasesSpeed)
                    ? 2 - component.MoodThresholds[MoodThreshold.Neutral] / component.CurrentMoodLevel
                    : 1,
            component.MinimumSpeedModifier,
            component.MaximumSpeedModifier);

        args.ModifySpeed(1, modifier);
    }

    private void OnMoodEffect(EntityUid uid, MoodComponent component, MoodEffectEvent args)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled)
            || !_prototypeManager.TryIndex<MoodEffectPrototype>(args.EffectId, out var prototype))
            return;

        var ev = new OnMoodEffect(uid, args.EffectId, args.EffectModifier, args.EffectOffset);
        RaiseLocalEvent(uid, ref ev);

        ApplyEffect(uid, component, prototype, ev.EffectModifier, ev.EffectOffset);
    }

    private void StartEffectTimeout(EntityUid uid, MoodComponent mood, MoodEffectPrototype effect)
    {
        if (effect.Timeout == 0)
            return;

        if (mood.EffectTimeoutSources.TryGetValue(effect.ID, out var oldTimeoutSource))
            DisposeEffectTimeout(oldTimeoutSource);

        var timeoutSource = new CancellationTokenSource();
        Timer.Spawn(
            TimeSpan.FromSeconds(effect.Timeout),
            () => RemoveTimedOutEffect(uid, effect.ID, effect.Category),
            timeoutSource.Token);

        mood.EffectTimeoutSources[effect.ID] = timeoutSource;
    }

    private static void DisposeEffectTimeout(CancellationTokenSource timeoutSource)
    {
        timeoutSource.Cancel();
        timeoutSource.Dispose();
    }

    private void StopEffectTimeout(MoodComponent mood, string effectId)
    {
        if (!mood.EffectTimeoutSources.Remove(effectId, out var timeoutSource))
            return;

        DisposeEffectTimeout(timeoutSource);
    }

    private void ApplyEffect(EntityUid uid, MoodComponent component, MoodEffectPrototype prototype, float eventModifier = 1, float eventOffset = 0)
    {
        var moodChange = prototype.MoodChange * eventModifier + eventOffset;

        if (prototype.Category != null)
        {
            if (component.CategorisedEffects.TryGetValue(prototype.Category, out var oldPrototypeId))
            {
                if (!_prototypeManager.TryIndex<MoodEffectPrototype>(oldPrototypeId, out var oldPrototype))
                    return;

                if (!component.CategorisedEffects.ContainsValue(prototype.ID))
                    SendEffectText(uid, prototype);

                if (prototype.ID != oldPrototype.ID)
                {
                    StopEffectTimeout(component, oldPrototype.ID);
                    component.CategorisedEffects[prototype.Category] = prototype.ID;
                }
            }
            else
            {
                SendEffectText(uid, prototype);
                component.CategorisedEffects.Add(prototype.Category, prototype.ID);
            }

            component.CategorisedEffectValues[prototype.Category] = moodChange;
            StartEffectTimeout(uid, component, prototype);
        }
        else
        {
            if (component.UncategorisedEffects.TryGetValue(prototype.ID, out _))
            {
                StartEffectTimeout(uid, component, prototype);
                RefreshMood(uid, component);
                return;
            }

            if (moodChange == 0)
                return;

            if (!component.UncategorisedEffects.ContainsKey(prototype.ID))
                SendEffectText(uid, prototype);

            component.UncategorisedEffects.Add(prototype.ID, moodChange);
            StartEffectTimeout(uid, component, prototype);
        }

        RefreshMood(uid, component);
    }

    private void SendEffectText(EntityUid uid, MoodEffectPrototype prototype)
    {
        if (prototype.Hidden)
            return;

        _popup.PopupEntity(prototype.Description, uid, uid, prototype.MoodChange >= 0 ? PopupType.Medium : PopupType.MediumCaution);
    }

    private void RemoveTimedOutEffect(EntityUid uid, string prototypeId, string? category = null)
    {
        if (!TryComp<MoodComponent>(uid, out var comp))
            return;

        StopEffectTimeout(comp, prototypeId);

        if (category == null)
        {
            if (!comp.UncategorisedEffects.Remove(prototypeId))
                return;
        }
        else
        {
            if (!comp.CategorisedEffects.TryGetValue(category, out var currentProtoId)
                || currentProtoId != prototypeId
                || !_prototypeManager.HasIndex<MoodEffectPrototype>(currentProtoId))
                return;

            comp.CategorisedEffects.Remove(category);
            comp.CategorisedEffectValues.Remove(category);
        }

        ReplaceMood(uid, prototypeId);
        RefreshMood(uid, comp);
    }

    private void ReplaceMood(EntityUid uid, string prototypeId)
    {
        if (!_prototypeManager.TryIndex<MoodEffectPrototype>(prototypeId, out var proto) || proto.MoodletOnEnd is null)
            return;

        EntityManager.EventBus.RaiseLocalEvent(uid, new MoodEffectEvent(proto.MoodletOnEnd.Value));
    }

    private void OnMobStateChanged(EntityUid uid, MoodComponent component, MobStateChangedEvent args)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled))
            return;

        if (args.NewMobState == MobState.Dead && args.OldMobState != MobState.Dead)
            RaiseLocalEvent(uid, new MoodEffectEvent("Dead"));
        else if (args.OldMobState == MobState.Dead && args.NewMobState != MobState.Dead)
            RaiseLocalEvent(uid, new MoodRemoveEffectEvent("Dead"));

        RefreshMood(uid, component);
    }

    private void RefreshMood(EntityUid uid, MoodComponent component)
    {
        var amount = 0f;

        foreach (var (category, protoId) in component.CategorisedEffects)
        {
            if (_prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var prototype))
                amount += component.CategorisedEffectValues.GetValueOrDefault(category, prototype.MoodChange);
        }

        foreach (var (_, value) in component.UncategorisedEffects)
            amount += value;

        SetMood(uid, amount, component, refresh: true);
    }

    private void OnInit(EntityUid uid, MoodComponent component, ComponentStartup args)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled))
            return;

        if (_config.GetCVar(CCVars.MoodModifiesThresholds)
            && TryComp<MobThresholdsComponent>(uid, out var mobThresholdsComponent)
            && _mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var critThreshold, mobThresholdsComponent))
            component.CritThresholdBeforeModify = critThreshold.Value;

        EnsureComp<NetMoodComponent>(uid);
        RefreshMood(uid, component);
    }

    private void SetMood(EntityUid uid, float amount, MoodComponent? component = null, bool force = false, bool refresh = false)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled)
            || !Resolve(uid, ref component)
            || component.CurrentMoodThreshold == MoodThreshold.Dead && !refresh)
            return;

        var neutral = component.MoodThresholds[MoodThreshold.Neutral];
        var ev = new OnSetMoodEvent(uid, amount, false);
        RaiseLocalEvent(uid, ref ev);

        if (ev.Cancelled)
            return;

        uid = ev.Receiver;
        amount = ev.MoodChangedAmount;

        var newMoodLevel = amount + neutral + ev.MoodOffset;
        if (!force)
        {
            newMoodLevel = Math.Clamp(
                newMoodLevel,
                component.MoodThresholds[MoodThreshold.Dead],
                component.MoodThresholds[MoodThreshold.Perfect]);
        }

        component.CurrentMoodLevel = newMoodLevel;

        if (TryComp<NetMoodComponent>(uid, out var mood))
        {
            mood.CurrentMoodLevel = component.CurrentMoodLevel;
            mood.NeutralMoodThreshold = component.MoodThresholds.GetValueOrDefault(MoodThreshold.Neutral);
            Dirty(uid, mood);
        }

        RefreshShaders(uid, component.CurrentMoodLevel);
        UpdateCurrentThreshold(uid, component);
    }

    private void UpdateCurrentThreshold(EntityUid uid, MoodComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var calculatedThreshold = GetMoodThreshold(component);
        if (calculatedThreshold == component.CurrentMoodThreshold)
            return;

        component.CurrentMoodThreshold = calculatedThreshold;
        DoMoodThresholdsEffects(uid, component);
    }

    private void DoMoodThresholdsEffects(EntityUid uid, MoodComponent? component = null, bool force = false)
    {
        if (!Resolve(uid, ref component) || component.CurrentMoodThreshold == component.LastThreshold && !force)
            return;

        var modifier = GetMovementThreshold(component.CurrentMoodThreshold);

        if (modifier != GetMovementThreshold(component.LastThreshold))
        {
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            SetCritThreshold(uid, component, modifier);
        }

        if (component.MoodThresholdsAlerts.TryGetValue(component.CurrentMoodThreshold, out var alertId))
            _alerts.ShowAlert(uid, alertId);
        else
            _alerts.ClearAlertCategory(uid, component.MoodCategory);

        component.LastThreshold = component.CurrentMoodThreshold;
    }

    private void RefreshShaders(EntityUid uid, float mood)
    {
        EnsureComp<SaturationScaleOverlayComponent>(uid, out var comp);
        comp.SaturationScale = mood / 50;
        Dirty(uid, comp);
    }

    private void SetCritThreshold(EntityUid uid, MoodComponent component, int modifier)
    {
        if (!_config.GetCVar(CCVars.MoodModifiesThresholds)
            || !TryComp<MobThresholdsComponent>(uid, out var mobThresholds)
            || !_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var key))
            return;

        var baseThreshold = component.CritThresholdBeforeModify == FixedPoint2.Zero
            ? key.Value
            : component.CritThresholdBeforeModify;

        var newKey = modifier switch
        {
            1 => FixedPoint2.New(baseThreshold.Float() * component.IncreaseCritThreshold),
            -1 => FixedPoint2.New(baseThreshold.Float() * component.DecreaseCritThreshold),
            _ => baseThreshold,
        };

        component.CritThresholdBeforeModify = baseThreshold;
        _mobThreshold.SetMobStateThreshold(uid, newKey, MobState.Critical, mobThresholds);
    }

    private MoodThreshold GetMoodThreshold(MoodComponent component, float? moodLevel = null)
    {
        moodLevel ??= component.CurrentMoodLevel;
        var result = MoodThreshold.Dead;
        var value = component.MoodThresholds[MoodThreshold.Perfect];

        foreach (var threshold in component.MoodThresholds)
        {
            if (threshold.Value > value || threshold.Value < moodLevel)
                continue;

            result = threshold.Key;
            value = threshold.Value;
        }

        return result;
    }

    private int GetMovementThreshold(MoodThreshold threshold) =>
        threshold switch
        {
            >= MoodThreshold.Good => 1,
            <= MoodThreshold.Meh => -1,
            _ => 0,
        };

    private void OnDamageChange(EntityUid uid, MoodComponent component, DamageChangedEvent args)
    {
        if (!_mobThreshold.TryGetPercentageForState(uid, MobState.Critical, args.Damageable.TotalDamage, out var damage))
            return;

        var protoId = "HealthNoDamage";
        var value = component.HealthMoodEffectsThresholds["HealthNoDamage"];

        foreach (var threshold in component.HealthMoodEffectsThresholds)
        {
            if (threshold.Value > damage || threshold.Value < value)
                continue;

            protoId = threshold.Key;
            value = threshold.Value;
        }

        RaiseLocalEvent(uid, new MoodEffectEvent(protoId));
    }
}
