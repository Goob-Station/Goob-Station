// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Chemistry.Visualizers;

/// <summary>
/// A component that changes color to match its contained reagents.
/// Managed by <see cref="SmokeVisualizerSystem"/>.
/// Only functions with smoke at the moment.
/// </summary>
[RegisterComponent]
[Access(typeof(SmokeVisualizerSystem))]
public sealed partial class SmokeVisualsComponent : Component {}