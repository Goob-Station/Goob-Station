# SPDX-FileCopyrightText: 2022 Eoin Mcloughlin <helloworld@eoinrul.es>
# SPDX-FileCopyrightText: 2022 Rinkashikachi <15rinkashikachi15@gmail.com>
# SPDX-FileCopyrightText: 2022 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
# SPDX-FileCopyrightText: 2023 Justin <justinly@usc.edu>
# SPDX-FileCopyrightText: 2023 Thom <119594676+ItsMeThom@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Crotalus <Crotalus@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

lathe-menu-title = Menu du tour
lathe-menu-queue = File d'attente
lathe-menu-server-list = Liste des serveurs
lathe-menu-sync = Synchroniser
lathe-menu-search-designs = Rechercher des modèles
lathe-menu-category-all = Tout
lathe-menu-search-filter = Filtre :
lathe-menu-amount = Quantité :
lathe-menu-recipe-count = { $count ->
    [1] {$count} Recette
    *[other] {$count} Recettes
}
lathe-menu-reagent-slot-examine = Il possède un emplacement pour un bécher sur le côté.
lathe-reagent-dispense-no-container = Du liquide coule de {THE($name)} sur le sol !
lathe-menu-result-reagent-display = {$reagent} ({$amount}u)
lathe-menu-material-display = {$material} ({$amount})
lathe-menu-tooltip-display = {$amount} de {$material}
lathe-menu-description-display = [italic]{$description}[/italic]
lathe-menu-material-amount = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit}
    *[other] {NATURALFIXED($amount, 2)} {$unit}
}
lathe-menu-material-amount-missing = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit} de {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {$unit} manquant[/color])
    *[other] {NATURALFIXED($amount, 2)} {$unit} de {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {$unit} manquants[/color])
}
lathe-menu-no-materials-message = Aucun matériau chargé.
lathe-menu-fabricating-message = Fabrication en cours...
lathe-menu-materials-title = Matériaux
lathe-menu-queue-title = File de fabrication
lathe-menu-queue-reset-title = Réinitialiser la file
lathe-menu-queue-reset-material-overflow = Vous remarquez que le tour automatique est plein.
