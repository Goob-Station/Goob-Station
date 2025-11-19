// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Client.Chemistry.Visualizers;

/// <summary>
/// A component that changes color to match its contained reagents.
/// Managed by <see cref="SmokeVisualizerSystem"/>.
/// Only functions with smoke at the moment.
/// </summary>
[RegisterComponent]
[Access(typeof(SmokeVisualizerSystem))]
public sealed partial class SmokeVisualsComponent : Component {}
