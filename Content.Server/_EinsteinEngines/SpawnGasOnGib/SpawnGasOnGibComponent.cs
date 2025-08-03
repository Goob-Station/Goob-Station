// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Server._EinsteinEngines.SpawnGasOnGib;

// <summary>
//   Spawns a gas mixture upon being gibbed.
// </summary>
[RegisterComponent]
public sealed partial class SpawnGasOnGibComponent : Component
{
    // <summary>
    //   The gas mixture to spawn.
    // </summary>
    [DataField("gasMixture", required: true)]
    public GasMixture Gas = new();
}
