// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised when shuttle console approved FTL
/// </summary>
[ByRefEvent]
public record struct ShuttleConsoleFTLTravelStartEvent(EntityUid Uid)
{
    public EntityUid Uid = Uid;
}