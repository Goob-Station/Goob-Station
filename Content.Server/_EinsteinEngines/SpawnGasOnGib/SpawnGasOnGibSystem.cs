// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Body.Events;

namespace Content.Server._EinsteinEngines.SpawnGasOnGib;

public sealed partial class SpawnGasOnGibSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnGasOnGibComponent, BeingGibbedEvent>(OnBeingGibbed);
    }

    private void OnBeingGibbed(EntityUid uid, SpawnGasOnGibComponent comp, BeingGibbedEvent args)
    {
        if (_atmos.GetContainingMixture(uid, false, true) is not { } air)
            return;

        _atmos.Merge(air, comp.Gas);
    }
}
