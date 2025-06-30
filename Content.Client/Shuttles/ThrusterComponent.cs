// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Client.Shuttles;

/// <summary>
/// A component that emits a visible exhaust plume if the entity is an active thruster.
/// Managed by <see cref="ThrusterSystem"/>
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ThrusterSystem))]
public sealed partial class ThrusterComponent : Component
{
}