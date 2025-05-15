# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
# SPDX-FileCopyrightText: 2022 TheDarkElites <73414180+TheDarkElites@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ike709 <ike709@github.com>
# SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
# SPDX-FileCopyrightText: 2023 Chronophylos <nikolai@chronophylos.com>
# SPDX-FileCopyrightText: 2023 Daniil Sikinami <60344369+VigersRay@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


### UI

# For the PDA screen
comp-pda-ui = ID: [color=white]{$owner}[/color], [color=yellow]{$jobTitle}[/color]

comp-pda-ui-blank = ID:

comp-pda-ui-owner = Owner: [color=white]{$actualOwnerName}[/color]

comp-pda-io-program-list-button = Programs

comp-pda-io-settings-button = Settings

comp-pda-io-program-fallback-title = Program

comp-pda-io-no-programs-available = No Programs Available

pda-bound-user-interface-show-uplink-title = Open Uplink
pda-bound-user-interface-show-uplink-description = Access your uplink

pda-bound-user-interface-lock-uplink-title = Lock Uplink
pda-bound-user-interface-lock-uplink-description = Prevent anyone from accessing your uplink without the code

comp-pda-ui-menu-title = PDA

comp-pda-ui-footer = Personal Digital Assistant

comp-pda-ui-station = Station: [color=white]{$station}[/color]

comp-pda-ui-station-alert-level = Alert Level: [color={ $color }]{ $level }[/color]

comp-pda-ui-station-alert-level-instructions = Instructions: [color=white]{ $instructions }[/color]

comp-pda-ui-station-time = Shift duration: [color=white]{ $time }[/color]

comp-pda-ui-eject-id-button = Eject ID

comp-pda-ui-eject-pen-button = Eject Pen

comp-pda-ui-ringtone-button = Ringtone

comp-pda-ui-ringtone-button-description = Change your PDA's ringtone

comp-pda-ui-toggle-flashlight-button = Toggle Flashlight

pda-bound-user-interface-music-button = Music Instrument

pda-bound-user-interface-music-button-description = Play music on your PDA

comp-pda-ui-unknown = Unknown

comp-pda-ui-unassigned = Unassigned

pda-notification-message = [font size=12][bold]PDA[/bold] { $header }: [/font]
    "{ $message }"
