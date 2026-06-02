# SPDX-FileCopyrightText: 2022 LittleBuilderJane <63973502+LittleBuilderJane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Myctai <108953437+Myctai@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2024 strO0pwafel <153459934+strO0pwafel@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Commands
## Delay shuttle round end
cmd-delayroundend-desc = Arrête le minuteur qui met fin à la partie lorsque la navette d'urgence sort de l'hyperespace.
cmd-delayroundend-help = Usage: delayroundend
emergency-shuttle-command-round-yes = Partie retardée.
emergency-shuttle-command-round-no = Impossible de retarder la fin de la partie.
## Dock emergency shuttle
cmd-dockemergencyshuttle-desc = Appelle la navette d'urgence et l'amarre à la station... si possible.
cmd-dockemergencyshuttle-help = Usage: dockemergencyshuttle
## Launch emergency shuttle
cmd-launchemergencyshuttle-desc = Lance la navette d'urgence en avance si possible.
cmd-launchemergencyshuttle-help = Usage: launchemergencyshuttle
# Emergency shuttle
emergency-shuttle-left = La navette d'urgence a quitté la station. Estimation de {$transitTime} secondes avant l'arrivée de la navette au Commandement Central.
emergency-shuttle-launch-time = La navette d'urgence décollera dans {$consoleAccumulator} secondes.
emergency-shuttle-docked = La navette d'urgence s'est amarrée au {$direction} de la station, {$location}. Elle partira dans {$time} secondes.{$extended}
emergency-shuttle-good-luck = La navette d'urgence est incapable de trouver une station. Bonne chance.
emergency-shuttle-nearby = La navette d'urgence est incapable de trouver un port d'amarrage valide. Elle a atterri au {$direction} de la station, {$location}. Elle partira dans {$time} secondes.{$extended}
emergency-shuttle-extended = {" "}Le temps de décollage a été prolongé en raison de circonstances défavorables.
# Emergency shuttle console popup / announcement
emergency-shuttle-console-no-early-launches = Le décollage anticipé est désactivé
emergency-shuttle-console-auth-left = {$remaining} autorisation(s) nécessaire(s) avant le décollage anticipé de la navette.
emergency-shuttle-console-auth-revoked = Autorisation de décollage anticipé révoquée, {$remaining} autorisation(s) nécessaire(s).
emergency-shuttle-console-denied = Accès refusé
# UI
emergency-shuttle-console-window-title = Console de la navette d'urgence
emergency-shuttle-ui-engines = MOTEURS :
emergency-shuttle-ui-idle = En veille
emergency-shuttle-ui-repeal-all = Tout révoquer
emergency-shuttle-ui-early-authorize = Autorisation de décollage anticipé
emergency-shuttle-ui-authorize = AUTORISER
emergency-shuttle-ui-repeal = RÉVOQUER
emergency-shuttle-ui-authorizations = Autorisations
emergency-shuttle-ui-remaining = Restantes : {$remaining}
# Map Misc.
map-name-centcomm = Commandement Central
map-name-terminal = Terminal des arrivées