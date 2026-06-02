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

shell-command-success = Commande réussie
shell-invalid-command = Commande invalide.
shell-invalid-command-specific = Commande {$commandName} invalide.
shell-cannot-run-command-from-server = Vous ne pouvez pas exécuter cette commande depuis le serveur.
shell-only-players-can-run-this-command = Seuls les joueurs peuvent exécuter cette commande.
shell-must-be-attached-to-entity = Vous devez être attaché à une entité pour exécuter cette commande.
shell-must-have-body = Vous devez avoir un corps pour exécuter cette commande.

## Arguments

shell-need-exactly-one-argument = Un seul argument est requis.
shell-wrong-arguments-number-need-specific = Il faut {$properAmount} arguments, il y en a {$currentAmount}.
shell-argument-must-be-number = L'argument doit être un nombre.
shell-argument-must-be-boolean = L'argument doit être un booléen.
shell-wrong-arguments-number = Nombre d'arguments incorrect.
shell-need-between-arguments = Il faut entre {$lower} et {$upper} arguments !
shell-need-minimum-arguments = Il faut au moins {$minimum} arguments !
shell-need-minimum-one-argument = Il faut au moins un argument !
shell-need-exactly-zero-arguments = Cette commande ne prend aucun argument.

shell-argument-uid = UID d'entité

## Guards

shell-missing-required-permission = Vous avez besoin de la permission {$perm} pour cette commande !
shell-entity-is-not-mob = L'entité cible n'est pas un être vivant !
shell-invalid-entity-id = Identifiant d'entité invalide.
shell-invalid-grid-id = Identifiant de grille invalide.
shell-invalid-map-id = Identifiant de carte invalide.
shell-invalid-entity-uid = {$uid} n'est pas un UID d'entité valide
shell-invalid-bool = Booléen invalide.
shell-entity-uid-must-be-number = L'UID d'entité doit être un nombre.
shell-could-not-find-entity = Impossible de trouver l'entité {$entity}
shell-could-not-find-entity-with-uid = Impossible de trouver une entité avec l'UID {$uid}
shell-entity-with-uid-lacks-component = L'entité avec l'UID {$uid} n'a pas de composant {INDEFINITE($componentName)} {$componentName}
shell-entity-target-lacks-component = L'entité cible n'a pas de composant {INDEFINITE($componentName)} {$componentName}
shell-invalid-color-hex = Code couleur hexadécimal invalide !
shell-target-player-does-not-exist = Le joueur cible n'existe pas !
shell-target-entity-does-not-have-message = L'entité cible n'a pas de {INDEFINITE($missing)} {$missing} !
shell-timespan-minutes-must-be-correct = {$span} n'est pas une durée en minutes valide.
shell-argument-must-be-prototype = L'argument {$index} doit être un {LOC($prototypeName)} !
shell-argument-number-must-be-between = L'argument {$index} doit être un nombre entre {$lower} et {$upper} !
shell-argument-station-id-invalid = L'argument {$index} doit être un identifiant de station valide !
shell-argument-map-id-invalid = L'argument {$index} doit être un identifiant de carte valide !
shell-argument-number-invalid = L'argument {$index} doit être un nombre valide !

# Hints
shell-argument-username-hint = <nom d'utilisateur>
shell-argument-username-optional-hint = [nom d'utilisateur]
