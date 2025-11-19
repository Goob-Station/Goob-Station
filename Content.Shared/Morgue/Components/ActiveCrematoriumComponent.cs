// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Morgue.Components;

/// <summary>
/// Used to track actively cooking crematoriums.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveCrematoriumComponent : Component;
