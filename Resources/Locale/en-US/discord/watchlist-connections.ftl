# SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
#
# SPDX-License-Identifier: MIT-WIZARDS

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
