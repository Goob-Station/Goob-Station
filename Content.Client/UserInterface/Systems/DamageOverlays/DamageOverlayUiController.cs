// SPDX-FileCopyrightText: 2023 Doru991 <75124791+Doru991@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Coolsurf6 <coolsurf24@yahoo.com.au>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.Assorted;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Player;

// Shitmed Change
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared.Body.Components;

namespace Content.Client.UserInterface.Systems.DamageOverlays;

[UsedImplicitly]
public sealed class DamageOverlayUiController : UIController
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    [UISystemDependency] private readonly ConsciousnessSystem _consciousness = default!; // Shitmed Change
    [UISystemDependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    private Overlays.DamageOverlay _overlay = default!;

    public override void Initialize()
    {
        _overlay = new Overlays.DamageOverlay();
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeNetworkEvent<MobThresholdChecked>(OnThresholdCheck); // Shitmed Change
    }

    private void OnPlayerAttach(LocalPlayerAttachedEvent args)
    {
        ClearOverlay();
        if (!EntityManager.TryGetComponent<MobStateComponent>(args.Entity, out var mobState))
            return;
        if (mobState.CurrentState != MobState.Dead)
            UpdateOverlays(args.Entity, mobState);
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
        ClearOverlay();
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.Target != _playerManager.LocalEntity)
            return;

        UpdateOverlays(args.Target, args.Component);
    }

    private void OnThresholdCheck(MobThresholdChecked args, EntitySessionEventArgs session)
    {
        if (!EntityManager.TryGetEntity(args.Uid, out var entity)
            || !_playerManager.LocalEntity.Equals(entity))
            return;

        UpdateOverlays(entity.Value);
    }

    private void ClearOverlay()
    {
        _overlay.DeadLevel = 0f;
        _overlay.CritLevel = 0f;
        _overlay.PainLevel = 0f;
        _overlay.OxygenLevel = 0f;
    }

    private void UpdateOverlays(EntityUid entity,
        MobStateComponent? mobState = null,
        BodyComponent? body = null,
        DamageableComponent? damageable = null,
        MobThresholdsComponent? thresholds = null)
    {
        if (mobState == null && !EntityManager.TryGetComponent(entity, out mobState) ||
            thresholds == null && !EntityManager.TryGetComponent(entity, out thresholds) ||
            body == null && !EntityManager.TryGetComponent(entity, out body)
            && damageable == null && !EntityManager.TryGetComponent(entity, out damageable))
            return;

        if (!thresholds.ShowOverlays)
        {
            ClearOverlay();
            return; //this entity intentionally has no overlays
        }

        _overlay.State = mobState.CurrentState;

        if (body == null && damageable != null)
        {
            if (!_mobThresholdSystem.TryGetIncapThreshold(entity, out var foundThreshold, thresholds))
                return; //this entity cannot die or crit!!

            var critThreshold = foundThreshold.Value;
            switch (mobState.CurrentState)
            {
                // Why the fuck is this the correct formatting??? Im gonna fucking kill someone.
                case MobState.Alive:
                    {
                        if (damageable.DamagePerGroup.TryGetValue("Brute", out var bruteDamage))
                            _overlay.PainLevel = FixedPoint2.Min(1f, bruteDamage / critThreshold).Float();

                        if (damageable.DamagePerGroup.TryGetValue("Airloss", out var oxyDamage))
                            _overlay.OxygenLevel = FixedPoint2.Min(1f, oxyDamage / critThreshold).Float();

                        if (_overlay.PainLevel < 0.05f) // Don't show damage overlay if they're near enough to max.
                            _overlay.PainLevel = 0;

                        _overlay.CritLevel = 0;
                        _overlay.DeadLevel = 0;
                        break;
                    }
                case MobState.Critical:
                    {
                        if (!_mobThresholdSystem.TryGetDeadPercentage(entity,
                                FixedPoint2.Max(0.0, _mobThresholdSystem.CheckVitalDamage(entity, damageable)), out var critLevel)) // GoobStation
                            return;
                        _overlay.CritLevel = critLevel.Value.Float();

                        _overlay.PainLevel = 0;
                        _overlay.DeadLevel = 0;
                        break;
                    }
                case MobState.Dead:
                    {
                        _overlay.PainLevel = 0;
                        _overlay.CritLevel = 0;
                        break;
                    }
            }
        }
        else if (body != null)
        {
            if (!EntityManager.TryGetComponent<ConsciousnessComponent>(entity, out var consciousness))
                return;

            switch (mobState.CurrentState)
            {
                // Why the fuck is this the correct formatting??? Im gonna fucking kill someone.
                case MobState.Alive:
                    {
                        _overlay.CritLevel = 0;
                        _overlay.DeadLevel = 0;

                        if (consciousness.Consciousness <= 0 || consciousness.Consciousness >= consciousness.Cap)
                        {
                            _overlay.PainLevel = 0;
                            return;
                        }

                        _overlay.PainLevel = FixedPoint2.Min(1f,
                            (consciousness.Cap - consciousness.Consciousness) / (consciousness.Cap - consciousness.Threshold))
                            .Float();

                        if (_consciousness.TryGetNerveSystem(_playerManager.LocalEntity!.Value, out var nerveSys) &&
                            _consciousness.TryGetConsciousnessModifier(_playerManager.LocalEntity!.Value, nerveSys.Value, out var modifier, "Suffocation"))
                        {
                            _overlay.OxygenLevel = FixedPoint2.Min(1f, modifier.Value.Change / (consciousness.Cap - consciousness.Threshold)).Float();
                        }

                        if (_overlay.PainLevel < 0.05f) // Don't show damage overlay if they're near enough to max.
                        {
                            _overlay.PainLevel = 0;
                        }

                        break;
                    }
                case MobState.Critical:
                    {
                        _overlay.CritLevel = FixedPoint2.Min(1f,
                            (consciousness.Threshold - consciousness.Consciousness) / consciousness.Threshold)
                            .Float();

                        _overlay.PainLevel = 0;
                        _overlay.DeadLevel = 0;
                        break;
                    }
                case MobState.Dead:
                    {
                        _overlay.PainLevel = 0;
                        _overlay.CritLevel = 0;
                        break;
                    }
            }
        }
    }
}
