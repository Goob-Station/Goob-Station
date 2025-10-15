# SPDX-FileCopyrightText: 2024 Celene <4323352+CuteMoonGod@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Hmeister <nathan.springfredfoxbon4@gmail.com>
# SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Veritius <veritiusgaming@gmail.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

execution-verb-name = Execute
execution-verb-message = Use your weapon to execute someone.

# All the below localisation strings have access to the following variables
# attacker (the person committing the execution)
# victim (the person being executed)
# weapon (the weapon used for the execution)

execution-popup-melee-initial-internal = You ready {THE($weapon)} against {THE($victim)}'s throat.
execution-popup-melee-initial-external = { CAPITALIZE(THE($attacker)) } readies {POSS-ADJ($attacker)} {$weapon} against the throat of {THE($victim)}.
execution-popup-melee-complete-internal = You slit the throat of {THE($victim)}!
execution-popup-melee-complete-external = { CAPITALIZE(THE($attacker)) } slits the throat of {THE($victim)}!

execution-popup-self-initial-internal = You ready {THE($weapon)} against your own throat.
execution-popup-self-initial-external = { CAPITALIZE(THE($attacker)) } readies {POSS-ADJ($attacker)} {$weapon} against their own throat.
execution-popup-self-complete-internal = You slit your own throat!
execution-popup-self-complete-external = { CAPITALIZE(THE($attacker)) } slits their own throat!

execution-popup-gun-initial-internal = You ready the muzzle of {THE($weapon)} against {$victim}'s head.
execution-popup-gun-initial-external = {$attacker} readies the muzzle of {THE($weapon)} against {$victim}'s head.
execution-popup-gun-complete-internal = You blast {$victim} in the head!
execution-popup-gun-complete-external = {$attacker} blasts {$victim} in the head!
execution-popup-gun-clumsy-internal = You miss {$victim}'s head and shoot your foot instead!
execution-popup-gun-clumsy-external = {$attacker} misses {$victim} and shoots {POSS-ADJ($attacker)} foot instead!
execution-popup-gun-empty = {CAPITALIZE(THE($weapon))} clicks.

suicide-popup-gun-initial-internal = You place the muzzle of {THE($weapon)} in your mouth.
suicide-popup-gun-initial-external = {$attacker} places the muzzle of {THE($weapon)} in {POSS-ADJ($attacker)} mouth.
suicide-popup-gun-complete-internal = You shoot yourself in the head!
suicide-popup-gun-complete-external = {$attacker} shoots {REFLEXIVE($attacker)} in the head!
