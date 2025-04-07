// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Explosion;

[RegisterComponent]
[Access(typeof(ClusterGrenadeVisualizerSystem))]
public sealed partial class ClusterGrenadeVisualsComponent : Component
{
    [DataField("state")]
    public string? State;
}