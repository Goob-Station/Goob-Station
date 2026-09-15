# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Morbo <exstrominer@gmail.com>
# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Interrobang01 <113810873+Interrobang01@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ZeroDayDaemon <60460608+ZeroDayDaemon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Arkanic <50847107+Arkanic@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Traitor

traitor-round-end-codewords = The codewords were: [color=White]{$codewords}[/color]
traitor-round-end-agent-name = a corporate spy

objective-issuer-syndicate = [color=orange]Rival Corporation[/color]
objective-issuer-unknown = Unknown

# Shown at the end of a round of Traitor

traitor-title = Corporate Spies
traitor-description = Rival corporate agents are embedded among the expedition.
traitor-not-enough-ready-players = Not enough players readied up for the game! There were {$readyPlayersCount} players readied up out of {$minimumPlayers} needed. Can't start Traitor.
traitor-no-one-ready = No players readied up! Can't start Traitor.

## TraitorDeathMatch
traitor-death-match-title = Traitor Deathmatch
traitor-death-match-description = Everyone's a traitor. Everyone wants each other dead.
traitor-death-match-station-is-too-unsafe-announcement = The station is too unsafe to continue. You have one minute.
traitor-death-match-end-round-description-first-line = The PDAs recovered afterwards...
traitor-death-match-end-round-description-entry = {$originalName}'s PDA, with {$tcBalance} TC

## TraitorRole

# TraitorRole
traitor-role-greeting =
    You are a [color=orange]Corporate Spy[/color] working for {$corporation} — a rival to Weyland-Yutani (Seegson, pre-merger Yutani, or similar).
    Your handlers want Xenomorph specimens, hive data, or Engineer Jockey technology from this Wild Lands site.
    Objectives and codewords are in the character menu. Use your uplink for mission gear.
    Do not let Wey-Yu security identify you.
traitor-role-codewords =
    Recognition phrases: [color=lightgray]
    {$codewords}.[/color]
    Use them carefully with other spies.
traitor-role-uplink-code =
    Set your ringtone to the notes [color = lightgray]{$code}[/color] to lock or unlock your uplink.
    Remember to lock it after, or the station's crew will easily open it too!
traitor-role-uplink-implant =
    Your uplink implant has been activated, access it from your hotbar.
    The uplink is secure unless someone removes it from your body.

# don't need all the flavour text for character menu
traitor-role-codewords-short =
    The codewords are:
    {$codewords}.

traitor-role-uplink-code-short = Your uplink code is {$code}. Set it as your PDA ringtone to access the black market.
traitor-role-uplink-pen-code-short = Your pen uplink code is {$code}. Spin the pen to unlock. Locks when closed.
traitor-role-uplink-implant-short = Your uplink was implanted. Access it from the action menu.

traitor-role-moreinfo =
    Find more information about your role in the character menu.

traitor-role-nouplink =
    Unfortunately you do not have access to the black market.

traitor-role-allegiances =
    Your allegiances:

traitor-role-notes =
    Additional notes:

