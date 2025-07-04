# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morber <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
# SPDX-FileCopyrightText: 2023 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### for technical and/or system messages

## General

shell-command-success = Command successful
shell-invalid-command = Invalid command.
shell-invalid-command-specific = Invalid {$commandName} command.
shell-cannot-run-command-from-server = You cannot run this command from the server.
shell-only-players-can-run-this-command = Only players can run this command.
shell-must-be-attached-to-entity = You must be attached to an entity to run this command.

## Arguments

shell-need-exactly-one-argument = Need exactly one argument.
shell-wrong-arguments-number-need-specific = Need {$properAmount} arguments, there were {$currentAmount}.
shell-argument-must-be-number = Argument must be a number.
shell-argument-must-be-boolean = Argument must be a boolean.
shell-wrong-arguments-number = Wrong number of arguments.
shell-need-between-arguments = Need {$lower} to {$upper} arguments!
shell-need-minimum-arguments = Need at least {$minimum} arguments!
shell-need-minimum-one-argument = Need at least one argument!

shell-argument-uid = EntityUid

## Guards

shell-entity-is-not-mob = Target entity is not a mob!
shell-invalid-entity-id = Invalid entity ID.
shell-invalid-grid-id = Invalid grid ID.
shell-invalid-map-id = Invalid map ID.
shell-invalid-entity-uid = {$uid} is not a valid entity uid
shell-invalid-bool = Invalid boolean.
shell-entity-uid-must-be-number = EntityUid must be a number.
shell-could-not-find-entity = Could not find entity {$entity}
shell-could-not-find-entity-with-uid = Could not find entity with uid {$uid}
shell-entity-with-uid-lacks-component = Entity with uid {$uid} doesn't have {INDEFINITE($componentName)} {$componentName} component
shell-invalid-color-hex = Invalid color hex!
shell-target-player-does-not-exist = Target player does not exist!
shell-target-entity-does-not-have-message = Target entity does not have {INDEFINITE($missing)} {$missing}!
shell-timespan-minutes-must-be-correct = {$span} is not a valid minutes timespan.
shell-argument-must-be-prototype = Argument {$index} must be a {LOC($prototypeName)}!
shell-argument-number-must-be-between = Argument {$index} must be a number between {$lower} and {$upper}!
shell-argument-station-id-invalid = Argument {$index} must be a valid station id!
shell-argument-map-id-invalid = Argument {$index} must be a valid map id!
shell-argument-number-invalid = Argument {$index} must be a valid number!

# Hints
shell-argument-username-hint = <username>
shell-argument-username-optional-hint = [username]
