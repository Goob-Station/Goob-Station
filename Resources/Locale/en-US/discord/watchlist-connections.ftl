# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Palladinium <patrick.chieppe@hotmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

discord-watchlist-connection-header =
    { $players ->
        [one] {$players} player on a watchlist has
        *[other] {$players} players on a watchlist have
    } connected to {$serverName}

discord-watchlist-connection-entry = - {$playerName} with message "{$message}"{ $expiry ->
        [0] {""}
        *[other] {" "}(expires <t:{$expiry}:R>)
    }{ $otherWatchlists ->
        [0] {""}
        [one] {" "}and {$otherWatchlists} other watchlist
        *[other] {" "}and {$otherWatchlists} other watchlists
    }
