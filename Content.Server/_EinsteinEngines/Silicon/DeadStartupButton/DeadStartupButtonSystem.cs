// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.Lightning;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Server._EinsteinEngines.Silicon.Charge;
using Content.Server.Lightning.Components; // Goobstation - Fix IPC shock loops
using Content.Server.Power.EntitySystems; // Goobstation - Energycrit
using Content.Shared._EinsteinEngines.Silicon.DeadStartupButton;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Electrocution;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

 // Goobstation - Revive notification
using Content.Server.EUI;
using Content.Shared.Mind;
using Content.Server.Ghost;
using Robust.Shared.Player;

namespace Content.Server._EinsteinEngines.Silicon.DeadStartupButton;

public sealed class DeadStartupButtonSystem : SharedDeadStartupButtonSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly SiliconChargeSystem _siliconChargeSystem = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly BatterySystem _battery = default!; // Goobstation - Energycrit

     // Goobstation - Revive notification
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeadStartupButtonComponent, OnDoAfterButtonPressedEvent>(OnDoAfter);
        SubscribeLocalEvent<DeadStartupButtonComponent, ElectrocutedEvent>(OnElectrocuted);
        SubscribeLocalEvent<DeadStartupButtonComponent, MobStateChangedEvent>(OnMobStateChanged);

    }

    private void OnDoAfter(EntityUid uid, DeadStartupButtonComponent comp, OnDoAfterButtonPressedEvent args)
    {
        if (args.Handled || args.Cancelled
            || !TryComp<MobStateComponent>(uid, out var mobStateComponent)
            || !_mobState.IsDead(uid, mobStateComponent)
            || !TryComp<MobThresholdsComponent>(uid, out var mobThresholdsComponent)
            || !TryComp<DamageableComponent>(uid, out var damageable))
            return;

        // Check if entity have critical state
        if (_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var criticalThreshold, mobThresholdsComponent)
            && damageable.TotalDamage < criticalThreshold)
        {
            _mobState.ChangeMobState(uid, MobState.Alive, mobStateComponent);
            return;
        }

        // Check if entity have dead state
        if (_mobThreshold.TryGetThresholdForState(uid, MobState.Dead, out var deadThreshold, mobThresholdsComponent)
            && damageable.TotalDamage < deadThreshold)
        {
            _mobState.ChangeMobState(uid, MobState.Alive, mobStateComponent);
            return;
        }

        _audio.PlayPvs(comp.BuzzSound, uid, AudioHelpers.WithVariation(0.05f, _robustRandom));
        _popup.PopupEntity(Loc.GetString("dead-startup-system-reboot-failed", ("target", MetaData(uid).EntityName)), uid);
        Spawn("EffectSparks", Transform(uid).Coordinates);
    }

    private void OnElectrocuted(EntityUid uid, DeadStartupButtonComponent comp, ElectrocutedEvent args)
    {
        if (HasComp<LightningComponent>(args.SourceUid) // Goobstation - Fix IPC shock loops.
            || !TryComp<MobStateComponent>(uid, out var mobStateComponent)
            || !_mobState.IsDead(uid, mobStateComponent)
            || !_siliconChargeSystem.TryGetSiliconBattery(uid, out var bateria, out var batteryEnt) // Goobstation - Added batteryEnt argument
            || bateria.CurrentCharge <= 0)
            return;

        _lightning.ShootRandomLightnings(uid, 2, 4);
        _battery.TryUseCharge(batteryEnt.Value, bateria.CurrentCharge); // Goobstation - Added batteryEnt argument
    }

    private void OnMobStateChanged(EntityUid uid, DeadStartupButtonComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            return;

        _popup.PopupEntity(Loc.GetString("dead-startup-system-reboot-success", ("target", MetaData(uid).EntityName)), uid);
        _audio.PlayPvs(comp.Sound, uid);

        // GoobStation - Revive notification
        if (_mind.TryGetMind(uid, out _, out var mind) &&
            _player.TryGetSessionById(mind.UserId, out var playerSession))
        {
            // notify them they're being revived
            if (mind.CurrentEntity != uid)
                _euiManager.OpenEui(new ReturnToBodyEui(mind, _mind, _player), playerSession);
        }
    }

}
