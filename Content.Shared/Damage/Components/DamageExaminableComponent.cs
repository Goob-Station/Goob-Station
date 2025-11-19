// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Shows a detailed examine window with this entity's damage stats when examined.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DamageExaminableComponent : Component;
