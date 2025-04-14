// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Threading;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Configures the <see cref="InactivityTimeRestartRuleSystem"/> game rule.
/// </summary>
[RegisterComponent]
public sealed partial class MaxTimeRestartRuleComponent : Component
{
    /// <summary>
    /// The max amount of time the round can last
    /// </summary>
    [DataField("roundMaxTime", required: true)]
    public TimeSpan RoundMaxTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The amount of time between the round completing and the lobby appearing.
    /// </summary>
    [DataField("roundEndDelay", required: true)]
    public TimeSpan RoundEndDelay = TimeSpan.FromSeconds(10);

    public CancellationTokenSource TimerCancel = new();
}