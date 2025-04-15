// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Beam;
using Content.Shared.Revenant.Components;
using Content.Shared.Revenant.EntitySystems;

namespace Content.Server.Revenant.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed class RevenantOverloadedLightsSystem : SharedRevenantOverloadedLightsSystem
{
    [Dependency] private readonly BeamSystem _beam = default!;

    protected override void OnZap(Entity<RevenantOverloadedLightsComponent> lights)
    {
        var component = lights.Comp;
        if (component.Target == null)
            return;

        var lxform = Transform(lights);
        var txform = Transform(component.Target.Value);

        if (!lxform.Coordinates.TryDistance(EntityManager, txform.Coordinates, out var distance))
            return;
        if (distance > component.ZapRange)
            return;

        _beam.TryCreateBeam(lights, component.Target.Value, component.ZapBeamEntityId, accumulateIndex: false); // Goob edit
    }
}