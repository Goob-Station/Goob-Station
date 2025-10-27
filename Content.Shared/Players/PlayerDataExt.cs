// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Player;

namespace Content.Shared.Players;

public static class PlayerDataExt
{
    /// <summary>
    ///     Gets the correctly cast instance of content player data from an engine player data storage.
    /// </summary>
    public static ContentPlayerData? ContentData(this SessionData data)
    {
        return (ContentPlayerData?) data.ContentDataUncast;
    }

    /// <summary>
    ///     Gets the correctly cast instance of content player data from an engine player data storage.
    /// </summary>
    public static ContentPlayerData? ContentData(this ICommonSession session)
    {
        return session.Data.ContentData();
    }

    /// <summary>
    ///     Gets the mind that is associated with this player.
    /// </summary>
    public static EntityUid? GetMind(this ICommonSession session)
    {
        return session.Data.ContentData()?.Mind;
    }
}