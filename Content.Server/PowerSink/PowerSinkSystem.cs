// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Skye <22365940+Skyedra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Mono <182929384+Monotheonist@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Examine;
using Robust.Shared.Utility;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Server.Power.EntitySystems;

namespace Content.Server.PowerSink
{
    public sealed class PowerSinkSystem : EntitySystem
    {
        /// <summary>
        /// Percentage of battery full to trigger the announcement warning at.
        /// </summary>
        private const float WarningMessageThreshold = 0.70f;

        private readonly float[] _warningSoundThresholds = new[] { .80f, .90f, .95f, .98f };

        /// <summary>
        /// Length of time to delay explosion from battery full state -- this is used to play
        /// a brief SFX winding up the explosion.
        /// </summary>
        /// <returns></returns>
        private readonly TimeSpan _explosionDelayTime = TimeSpan.FromSeconds(1.465);

        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly BatterySystem _battery = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PowerSinkComponent, ExaminedEvent>(OnExamine);
        }

        private void OnExamine(EntityUid uid, PowerSinkComponent component, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange || !TryComp<PowerConsumerComponent>(uid, out var consumer))
                return;

            var drainAmount = (int) consumer.NetworkLoad.ReceivingPower / 1000;
            args.PushMarkup(
                Loc.GetString(
                    "powersink-examine-drain-amount",
                    ("amount", drainAmount),
                    ("markupDrainColor", "orange"))
            );
        }

        public override void Update(float frameTime)
        {
            var toRemove = new RemQueue<(EntityUid Entity, PowerSinkComponent Sink)>();
            var query = EntityQueryEnumerator<PowerSinkComponent, PowerConsumerComponent, BatteryComponent, TransformComponent>();

            // Realistically it's gonna be like <5 per station.
            while (query.MoveNext(out var entity, out var component, out var networkLoad, out var battery, out var transform))
            {
                if (!transform.Anchored)
                    continue;

                _battery.SetCharge(entity, battery.CurrentCharge + networkLoad.NetworkLoad.ReceivingPower / 1000, battery);

                var currentBatteryThreshold = battery.CurrentCharge / battery.MaxCharge;

                // Check for warning message threshold
                if (!component.SentImminentExplosionWarningMessage &&
                    currentBatteryThreshold >= WarningMessageThreshold)
                {
                    NotifyStationOfImminentExplosion(entity, component);
                }

                // Check for warning sound threshold
                foreach (var testThreshold in _warningSoundThresholds)
                {
                    if (currentBatteryThreshold >= testThreshold &&
                        testThreshold > component.HighestWarningSoundThreshold)
                    {
                        component.HighestWarningSoundThreshold = currentBatteryThreshold; // Don't re-play in future until next threshold hit
                        _audio.PlayPvs(component.ElectricSound, entity); // Play SFX
                        break;
                    }
                }

                // Check for explosion
                if (battery.CurrentCharge < battery.MaxCharge)
                    continue;

                if (component.ExplosionTime == null)
                {
                    // Set explosion sequence to start soon
                    component.ExplosionTime = _gameTiming.CurTime.Add(_explosionDelayTime);

                    // Wind-up SFX
                    _audio.PlayPvs(component.ChargeFireSound, entity); // Play SFX
                }
                else if (_gameTiming.CurTime >= component.ExplosionTime)
                {
                    // Explode!
                    toRemove.Add((entity, component));
                }
            }

            foreach (var (entity, component) in toRemove)
            {
                _explosionSystem.QueueExplosion(entity, "PowerSink", 2000f, 4f, 20f, canCreateVacuum: true);
                EntityManager.RemoveComponent(entity, component);
            }
        }

        private void NotifyStationOfImminentExplosion(EntityUid uid, PowerSinkComponent powerSinkComponent)
        {
            if (powerSinkComponent.SentImminentExplosionWarningMessage)
                return;

            powerSinkComponent.SentImminentExplosionWarningMessage = true;
            var station = _station.GetOwningStation(uid);

            if (station == null)
                return;

            _chat.DispatchStationAnnouncement(
                station.Value,
                Loc.GetString("powersink-imminent-explosion-announcement"),
                playDefaultSound: true,
                colorOverride: Color.Yellow
            );
        }
    }
}