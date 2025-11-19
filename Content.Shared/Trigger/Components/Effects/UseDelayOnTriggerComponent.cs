// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Timing;
using Robust.Shared.GameStates;

namespace Content.Shared.Trigger.Components.Effects;

/// <summary>
/// Will activate an UseDelay on the target when triggered.
/// </summary>
/// <remarks>
/// TODO: Support specific UseDelay IDs for each trigger key.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UseDelayOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// The UseDelay Id to delay.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string UseDelayId = UseDelaySystem.DefaultId;

    /// <summary>
    /// If true ongoing delays won't be reset.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CheckDelayed;
}
