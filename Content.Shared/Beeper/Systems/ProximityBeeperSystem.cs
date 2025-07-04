// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Beeper.Components;
using Content.Shared.ProximityDetection;

namespace Content.Shared.Beeper.Systems;

/// <summary>
/// This handles controlling a beeper from proximity detector events.
/// </summary>
public sealed class ProximityBeeperSystem : EntitySystem
{
    [Dependency] private readonly BeeperSystem _beeper = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ProximityBeeperComponent, NewProximityTargetEvent>(OnNewProximityTarget);
        SubscribeLocalEvent<ProximityBeeperComponent, ProximityTargetUpdatedEvent>(OnProximityTargetUpdate);
    }

    private void OnProximityTargetUpdate(EntityUid owner, ProximityBeeperComponent proxBeeper, ref ProximityTargetUpdatedEvent args)
    {
        if (!TryComp<BeeperComponent>(owner, out var beeper))
            return;
        if (args.Target == null)
        {
            _beeper.SetMute(owner, true, beeper);
            return;
        }

        _beeper.SetIntervalScaling(owner, args.Distance / args.Detector.Range, beeper);
        _beeper.SetMute(owner, false, beeper);
    }

    private void OnNewProximityTarget(EntityUid owner, ProximityBeeperComponent proxBeeper, ref NewProximityTargetEvent args)
    {
        _beeper.SetMute(owner, args.Target == null);
    }
}