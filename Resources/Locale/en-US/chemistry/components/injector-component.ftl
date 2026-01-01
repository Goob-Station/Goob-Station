# SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## UI

injector-volume-transfer-label = Volume: [color=white]{$currentVolume}/{$totalVolume}u[/color]
    Mode: [color=white]{$modeString}[/color] ([color=white]{$transferVolume}u[/color])
injector-volume-label = Volume: [color=white]{$currentVolume}/{$totalVolume}u[/color]
    Mode: [color=white]{$modeString}[/color]
injector-toggle-verb-text = Toggle Injector Mode

## Entity

injector-component-inject-mode-name = inject
injector-component-draw-mode-name = draw
injector-component-dynamic-mode-name = dynamic
injector-component-mode-changed-text = Now {$mode}
injector-component-transfer-success-message = You transfer {$amount}u into {THE($target)}.
injector-component-transfer-success-message-self = You transfer {$amount}u into yourself.
injector-component-inject-success-message = You inject {$amount}u into {THE($target)}!
injector-component-inject-success-message-self = You inject {$amount}u into yourself!
injector-component-draw-success-message = You draw {$amount}u from {THE($target)}.
injector-component-draw-success-message-self = You draw {$amount}u from yourself.

## Fail Messages

injector-component-target-already-full-message = {CAPITALIZE(THE($target))} is already full!
injector-component-target-already-full-message-self = You are already full!
injector-component-target-is-empty-message = {CAPITALIZE(THE($target))} is empty!
injector-component-target-is-empty-message-self = You are empty!
injector-component-cannot-toggle-draw-message = Too full to draw!
injector-component-cannot-toggle-inject-message = Nothing to inject!
injector-component-cannot-toggle-dynamic-message = Can't toggle dynamic!
injector-component-empty-message = {CAPITALIZE(THE($injector))} is empty!
injector-component-blocked-user = Protective gear blocked your injection!
injector-component-blocked-other = {CAPITALIZE(THE(POSS-ADJ($target)))} armor blocked {THE($user)}'s injection!
injector-component-cannot-transfer-message = You aren't able to transfer into {THE($target)}!
injector-component-cannot-transfer-message-self = You aren't able to transfer into yourself!
injector-component-cannot-inject-message = You aren't able to inject into {THE($target)}!
injector-component-cannot-inject-message-self = You aren't able to inject into yourself!
injector-component-cannot-draw-message = You aren't able to draw from {THE($target)}!
injector-component-cannot-draw-message-self = You aren't able to draw from yourself!
injector-component-ignore-mobs = This injector can only interact with containers!

## mob-inject doafter messages

injector-component-needle-injecting-user = You start injecting the needle.
injector-component-needle-injecting-target = {CAPITALIZE(THE($user))} is trying to inject a needle into you!
injector-component-needle-drawing-user = You start drawing the needle.
injector-component-needle-drawing-target = {CAPITALIZE(THE($user))} is trying to use a needle to draw from you!
injector-component-spray-injecting-user = You start preparing the spray nozzle.
injector-component-spray-injecting-target = {CAPITALIZE(THE($user))} is trying to place a spray nozzle onto you!

## Target Popup Success messages
injector-component-feel-prick-message = You feel a tiny prick!

# Goob
injector-component-deny-user = Exoskeleton too thick!
