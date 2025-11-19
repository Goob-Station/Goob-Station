// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

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
