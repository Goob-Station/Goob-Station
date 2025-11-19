// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.GameStates;

namespace Content.Shared.Survivor.Components;

/// <summary>
///     Component to keep track of which entities are a Survivor antag.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurvivorComponent : Component;
