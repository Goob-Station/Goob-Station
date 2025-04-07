// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Atmos.Reactions;

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