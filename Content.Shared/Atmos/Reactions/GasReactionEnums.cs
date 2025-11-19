// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Shared.Atmos.Reactions;

[Flags]
public enum ReactionResult : byte
{
    NoReaction = 0,
    Reacting = 1,
    StopReactions = 2,
}

public enum GasReaction : byte
{
    Fire = 0,
}
