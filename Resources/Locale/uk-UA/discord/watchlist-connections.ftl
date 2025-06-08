# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Palladinium <patrick.chieppe@hotmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

discord-watchlist-connection-header = { $players ->
        [one] {$players} гравець зі списку спостереження під'єднався
        *[other] {$players} гравців зі списку спостереження під'єдналися
    } до {$serverName}

discord-watchlist-connection-entry = - {$playerName} з повідомленням ""{$message}""{ $expiry ->
        [0] {""""}
        *[other] {"" ""}(закінчується <t:{$expiry}:R>)
    }{ $otherWatchlists ->
        [0] {""""}
        [one] {"" ""}та ще {$otherWatchlists} інший список спостереження
        *[other] {"" ""}та ще {$otherWatchlists} інших списків спостереження
    }
