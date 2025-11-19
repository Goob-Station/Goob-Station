// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Objectives.Components;

/// <summary>
/// Sets this objective's target to the one given in <see cref="TargetOverrideComponent"/>, if the entity has it.
/// This component needs to be added to objective entity itself.
/// </summary>
[RegisterComponent]
public sealed partial class PickSpecificPersonComponent : Component;
