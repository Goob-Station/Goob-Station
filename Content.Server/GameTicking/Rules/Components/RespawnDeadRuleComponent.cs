// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// This is used for gamemodes that automatically respawn players when they're no longer alive.
/// </summary>
[RegisterComponent, Access(typeof(RespawnRuleSystem))]
public sealed partial class RespawnDeadRuleComponent : Component
{
    /// <summary>
    /// Whether or not we want to add everyone who dies to the respawn tracker
    /// </summary>
    [DataField]
    public bool AlwaysRespawnDead;
}