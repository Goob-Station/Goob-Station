// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Spawners;
using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Goobstation.Server.Atmos.Components;

namespace Content.Goobstation.Server.Atmos.EntitySystems;


/// <summary>
/// Assmos - Extinguisher Nozzle
/// Sets atmospheric temperature to 20C and removes all toxins.
/// </summary>
public sealed class AtmosResinDespawnSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmo = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AtmosResinDespawnComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(EntityUid uid, AtmosResinDespawnComponent comp, ref TimedDespawnEvent args)
    {
        if (!TryComp(uid, out TransformComponent? xform))
            return;

        var mix = _atmo.GetContainingMixture(uid, true);
        GasMixture newMix = new();

        if (mix is null) return;
        newMix.AdjustMoles(0, mix.GetMoles(0));
        newMix.AdjustMoles(1, mix.GetMoles(1));

        mix.Remove(mix.TotalMoles);

        _atmo.Merge(mix, newMix);

        mix.Temperature = Atmospherics.T20C;
        _gasOverlaySystem.UpdateSessions();
    }
}
