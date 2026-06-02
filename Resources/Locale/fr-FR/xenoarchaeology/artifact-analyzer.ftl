# SPDX-FileCopyrightText: 2023 Guillaume E <262623+quatre@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vascreeper@yahoo.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
# SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

analysis-console-menu-title = Console d'analyse Marque 3 large spectre
analysis-console-server-list-button = Serveur
analysis-console-extract-button = Extraire les points

analysis-console-info-no-scanner = Aucun analyseur connecté ! Veuillez en connecter un avec un multioutil.
analysis-console-info-no-artifact = Aucun artefact présent ! Placez-en un sur le pad pour voir les informations des nœuds.
analysis-console-info-ready = Systèmes opérationnels. Prêt à scanner.

analysis-console-no-node = Sélectionner un nœud à afficher
analysis-console-info-id = [font="Monospace" size=11]ID :[/font]
analysis-console-info-id-value = [font="Monospace" size=11][color=yellow]{$id}[/color][/font]
analysis-console-info-class = [font="Monospace" size=11]Classe :[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{$class}[/font]
analysis-console-info-locked = [font="Monospace" size=11]Statut :[/font]
analysis-console-info-locked-value = [font="Monospace" size=11][color={ $state ->
    [0] red]Verrouillé
    [1] lime]Déverrouillé
    *[2] plum]Active
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Durabilité :[/font]
analysis-console-info-durability-value = [font="Monospace" size=11][color={$color}]{$current}/{$max}[/color][/font]
analysis-console-info-effect = [font="Monospace" size=11]Effet :[/font]
analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [true] {$info}
    *[false] Déverrouillez les nœuds pour obtenir des informations
}[/color][/font]
analysis-console-info-trigger = [font="Monospace" size=11]Déclencheurs :[/font]
analysis-console-info-triggered-value = [font="Monospace" size=11][color=gray]{$triggers}[/color][/font]
analysis-console-info-scanner = Scan en cours...
analysis-console-info-scanner-paused = En pause.
analysis-console-progress-text = {$seconds ->
    [one] T-{$seconds} seconde
    *[other] T-{$seconds} secondes
}

analysis-console-extract-value = [font="Monospace" size=11][color=orange]Nœud {$id} (+{$value})[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange] Aucun nœud déverrouillé n'a de points à extraire [/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Recherche totale : {$value}[/color][/font]

analyzer-artifact-extract-popup = De l'énergie scintille à la surface de l'artefact !
