// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Shared.Network;

namespace Content.Server.Connection.Whitelist.Conditions;

/// <summary>
/// Condition that matches if the player has played for a certain amount of time.
/// </summary>
public sealed partial class ConditionPlaytime : WhitelistCondition
{
    [DataField]
    public int MinimumPlaytime = 0; // In minutes
}
