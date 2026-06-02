# SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# FTLdiskburner
cmd-ftldisk-desc = Crée un disque de coordonnées FTL pour naviguer vers la carte sur laquelle se trouve l'EntityID donné
cmd-ftldisk-help = ftldisk [EntityID]
cmd-ftldisk-no-transform = L'entité {$destination} n'a pas de composant Transform !
cmd-ftldisk-no-map = L'entité {$destination} n'a pas de carte !
cmd-ftldisk-no-map-comp = L'entité {$destination} se trouve d'une façon ou d'une autre sur la carte {$map} sans composant de carte.
cmd-ftldisk-map-not-init = L'entité {$destination} se trouve sur la carte {$map} qui n'est pas initialisée ! Vérifiez qu'il est sûr de l'initialiser, puis initialisez d'abord la carte ou les joueurs seront bloqués sur place !
cmd-ftldisk-map-paused = L'entité {$desintation} se trouve sur la carte {$map} qui est en pause ! Veuillez d'abord reprendre la carte ou les joueurs seront bloqués sur place.
cmd-ftldisk-planet = L'entité {$desintation} se trouve sur la carte planétaire {$map} et nécessitera un point FTL. Il existe peut-être déjà.
cmd-ftldisk-already-dest-not-enabled = L'entité {$destination} se trouve sur la carte {$map} qui possède déjà un FTLDestinationComponent, mais il n'est pas activé ! Définissez ceci manuellement pour plus de sécurité.
cmd-ftldisk-requires-ftl-point = L'entité {$destination} se trouve sur la carte {$map} qui nécessite un point FTL pour s'y rendre ! Il existe peut-être déjà.
cmd-ftldisk-hint = netID de la carte
