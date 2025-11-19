// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// This is used for entities which shouldn't have their local rotation set when moving, e.g. those using
/// <see cref="MouseRotator"/> instead
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NoRotateOnMoveComponent : Component
{
}
