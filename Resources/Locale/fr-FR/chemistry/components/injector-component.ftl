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

injector-draw-text = Prélever
injector-inject-text = Injecter
injector-invalid-injector-toggle-mode = Invalide
injector-volume-label = Volume : [color=white]{$currentVolume}/{$totalVolume}[/color]
    Mode : [color=white]{$modeString}[/color] ([color=white]{$transferVolume}u[/color])

## Entity

injector-component-drawing-text = Prélèvement en cours
injector-component-injecting-text = Injection en cours
injector-component-cannot-transfer-message = Vous ne pouvez pas transférer vers {THE($target)} !
injector-component-cannot-draw-message = Vous ne pouvez pas prélever depuis {THE($target)} !
injector-component-cannot-inject-message = Vous ne pouvez pas injecter dans {THE($target)} !
injector-component-inject-success-message = Vous injectez {$amount}u dans {THE($target)} !
injector-component-transfer-success-message = Vous transférez {$amount}u dans {THE($target)}.
injector-component-draw-success-message = Vous prélevez {$amount}u depuis {THE($target)}.
injector-component-target-already-full-message = {CAPITALIZE(THE($target))} est déjà plein !
injector-component-target-is-empty-message = {CAPITALIZE(THE($target))} est vide !
injector-component-cannot-toggle-draw-message = Trop plein pour prélever !
injector-component-cannot-toggle-inject-message = Rien à injecter !

## mob-inject doafter messages

injector-component-drawing-user = Vous commencez à prélever avec l'aiguille.
injector-component-injecting-user = Vous commencez à injecter avec l'aiguille.
injector-component-drawing-target = {CAPITALIZE(THE($user))} essaie d'utiliser une aiguille pour prélever sur vous !
injector-component-injecting-target = {CAPITALIZE(THE($user))} essaie de vous injecter une aiguille !
injector-component-deny-user = Exosquelette trop épais !
