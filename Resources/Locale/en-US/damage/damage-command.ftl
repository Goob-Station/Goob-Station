# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Damage command loc.

damage-command-description = Add or remove damage to an entity. 
damage-command-help = Usage: {$command} <type/group> <amount> [ignoreResistances] [uid]

damage-command-arg-type = <damage type or group>
damage-command-arg-quantity = [quantity]
damage-command-arg-target = [target euid]

damage-command-error-type = {$arg} is not a valid damage group or type.
damage-command-error-euid = {$arg} is not a valid entity uid.
damage-command-error-quantity = {$arg} is not a valid quantity.
damage-command-error-bool = {$arg} is not a valid bool.
damage-command-error-player = No entity attached to session. You must specify a target uid
damage-command-error-args = Invalid number of arguments 