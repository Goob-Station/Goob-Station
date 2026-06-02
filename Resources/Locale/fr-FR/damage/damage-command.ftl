# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Damage command loc.

damage-command-description = Ajouter ou enlever des dégâts à une entité. 
damage-command-help = Usage : {$command} <type/group> <amount> [ignoreResistances] [uid]

damage-command-arg-type = <damage type or group>
damage-command-arg-quantity = [quantity]
damage-command-arg-target = [target euid]

damage-command-error-type = {$arg} n'est pas un groupe ou type de dégâts valide.
damage-command-error-euid = {$arg} n'est pas une uid d'entité valide.
damage-command-error-quantity = {$arg} n'est pas une quantité valide.
damage-command-error-bool = {$arg} n'est pas un bool valide.
damage-command-error-player = Pas d'entité attachée à la session. Vous devez spécifier l'uid d'une cible.
damage-command-error-args = Nombre d'arguments invalide 