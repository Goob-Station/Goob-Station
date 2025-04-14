// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Threading;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Gamerule that ends the round after a period of inactivity.
/// </summary>
[RegisterComponent, Access(typeof(InactivityTimeRestartRuleSystem))]
public sealed partial class InactivityRuleComponent : Component
{
    /// <summary>
    /// How long the round must be inactive to restart
    /// </summary>
    [DataField("inactivityMaxTime", required: true)]
    public TimeSpan InactivityMaxTime = TimeSpan.FromMinutes(10);

    /// <summary>
    /// The delay between announcing round end and the lobby.
    /// </summary>
    [DataField("roundEndDelay", required: true)]
    public TimeSpan RoundEndDelay  = TimeSpan.FromSeconds(10);

    public CancellationTokenSource TimerCancel = new();
}