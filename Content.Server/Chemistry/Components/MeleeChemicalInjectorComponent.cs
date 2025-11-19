// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Server.Chemistry.Components;

/// <summary>
/// Used for melee weapon entities that should try to inject a
/// contained solution into a target when used to hit it.
/// </summary>
[RegisterComponent]
public sealed partial class MeleeChemicalInjectorComponent : BaseSolutionInjectOnEventComponent { }
