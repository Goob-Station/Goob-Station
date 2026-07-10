// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.StepTrigger.Components;

/// <summary>
/// This is used for marking step trigger events that require the user to wear shoes or have protection of some sort, such as for glass shards.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PreventableStepTriggerComponent : Component;