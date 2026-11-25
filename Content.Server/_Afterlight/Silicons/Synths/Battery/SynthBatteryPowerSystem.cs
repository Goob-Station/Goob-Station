using Content.Shared._Afterlight.Silicons.Synths.Battery;
using Content.Shared.Damage.Systems;
using Content.Shared.Emp;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Server._Afterlight.Silicons.Synths.Battery;

public sealed partial class SynthBatteryPowerSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SynthBatteryAlertSystem _alerts = default!;
    [Dependency] private SynthBatteryEffectsSystem _effects = default!;
    [Dependency] private SynthBatterySystem _synthBattery = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SynthPowerCellComponent, ChargeChangedEvent>(OnCellChargeChanged);
        SubscribeLocalEvent<SynthBatteryComponent, EmpPulseEvent>(OnEmpPulse);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SynthBatteryComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var synth, out var mobState))
        {
            if (mobState.CurrentState == MobState.Dead || _timing.CurTime < synth.NextUpdate)
                continue;

            synth.NextUpdate = _timing.CurTime + synth.UpdateRate;
            UpdatePower((uid, synth), (float) synth.UpdateRate.TotalSeconds);
        }
    }

    private void UpdatePower(Entity<SynthBatteryComponent> ent, float delta)
    {
        if (!_synthBattery.TryGetBattery(ent.Owner, out var battery))
        {
            _alerts.UpdateBatteryAlert(ent);
            SetUnpowered(ent, true);
            return;
        }

        var cell = battery.Value.AsNullable();
        if (_battery.GetCharge(cell) <= 0f)
        {
            _alerts.UpdateBatteryAlert(ent);
            SetUnpowered(ent, true);
            return;
        }

        SetUnpowered(ent, false);

        if (ent.Comp.DrawRate <= 0f)
        {
            _alerts.UpdateBatteryAlert(ent);
            return;
        }

        var oldPercent = _battery.GetChargeLevel(cell) * 100f;
        if (_battery.ChangeCharge(cell, -ent.Comp.DrawRate * delta) < 0f)
            UpdateLowPowerWarning(ent, oldPercent, _battery.GetChargeLevel(cell) * 100f);
    }

    private void UpdateLowPowerWarning(Entity<SynthBatteryComponent> ent, float oldPercent, float newPercent)
    {
        foreach (var threshold in ent.Comp.WarningPercentages)
        {
            if (oldPercent <= threshold || newPercent > threshold)
                continue;

            if (ent.Comp.BatteryLowText is { } text)
                _popup.PopupEntity(Loc.GetString(text), ent, ent, PopupType.LargeCaution);

            PlaySound(ent, ent.Comp.BatteryLowSound);
            return;
        }
    }

    private void SetUnpowered(Entity<SynthBatteryComponent> ent, bool unpowered)
    {
        if (ent.Comp.Unpowered == unpowered)
            return;

        _effects.SetUnpowered(ent, unpowered);

        if (!unpowered)
            return;

        if (ent.Comp.BatteryDeadText is { } text)
            _popup.PopupEntity(Loc.GetString(text), ent, ent, PopupType.LargeCaution);

        PlaySound(ent, ent.Comp.BatteryDeadSound);
    }

    private void PlaySound(Entity<SynthBatteryComponent> ent, SoundSpecifier? sound)
    {
        if (sound != null)
            _audio.PlayPvs(sound, ent);
    }

    private void OnCellChargeChanged(Entity<SynthPowerCellComponent> ent, ref ChargeChangedEvent args)
    {
        if (!_synthBattery.TryGetSynth(ent.Owner, out var synth)
            || TryComp(synth.Owner, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
            return;

        SetUnpowered(synth, args.CurrentCharge <= 0f);
        _alerts.UpdateBatteryAlert(synth);
    }

    private void OnEmpPulse(Entity<SynthBatteryComponent> ent, ref EmpPulseEvent args)
    {
        args.Affected = true;

        if (ent.Comp.EmpDamage is { } damage)
            _damageable.TryChangeDamage(ent.Owner, damage, origin: args.User);

        if (_synthBattery.TryGetBattery(ent.Owner, out var battery))
            _battery.UseCharge(battery.Value.AsNullable(), args.EnergyConsumption);

        if (ent.Comp.BatteryEmpText is { } text)
            _popup.PopupEntity(Loc.GetString(text), ent, ent, PopupType.LargeCaution);
    }
}
