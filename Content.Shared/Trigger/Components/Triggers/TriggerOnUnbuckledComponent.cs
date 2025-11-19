// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Trigger.Components.Triggers;

/// <summary>
/// Triggers when the owning entity is unbuckled.
/// This is intended to be used on buckle-able entities like mobs.
/// The user is the strap entity (a chair or similar).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnUnbuckledComponent : BaseTriggerOnXComponent;
