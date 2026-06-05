// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Trigger.Components;

/// <summary>
/// This is used for randomizing a <see cref="TimerTriggerComponent"/> on MapInit.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RandomTimerTriggerComponent : Component
{
    /// <summary>
    /// The minimum random trigger time.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Min;

    /// <summary>
    /// The maximum random trigger time.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Max;
}
