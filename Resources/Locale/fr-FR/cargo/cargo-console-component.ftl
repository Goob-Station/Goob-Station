# SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Marat Gadzhiev <15rinkashikachi15@gmail.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2024 Andrew <blackledgecreates@gmail.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 icekot8 <93311212+icekot8@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## UI
cargo-console-menu-title = Console de commandes du cargo
cargo-console-menu-flavor-left = Commandez encore plus de boîtes à pizza que d'habitude !
cargo-console-menu-flavor-right = v2.1
cargo-console-menu-account-name-label = Compte :{" "}
cargo-console-menu-account-name-none-text = Aucun
cargo-console-menu-account-name-format = [bold][color={$color}]{$name}[/color][/bold] [font="Monospace"]\[{$code}\][/font]
cargo-console-menu-shuttle-name-label = Nom de la navette :{" "}
cargo-console-menu-shuttle-name-none-text = Aucune
cargo-console-menu-points-label = Solde :{" "}
cargo-console-menu-points-amount = ${$amount}
cargo-console-menu-shuttle-status-label = État de la navette :{" "}
cargo-console-menu-shuttle-status-away-text = En déplacement
cargo-console-menu-order-capacity-label = Capacité de commande :{" "}
cargo-console-menu-call-shuttle-button = Activer le télépad
cargo-console-menu-permissions-button = Permissions
cargo-console-menu-categories-label = Catégories :{" "}
cargo-console-menu-search-bar-placeholder = Rechercher
cargo-console-menu-requests-label = Demandes
cargo-console-menu-orders-label = Commandes
cargo-console-menu-order-row-title = {$productName} (x{$orderAmount} pour {$orderPrice}$)
cargo-console-menu-populate-orders-cargo-order-row-product-name-text = Demandé par : {$orderRequester} depuis [color={$accountColor}]{$account}[/color]
cargo-console-menu-order-row-product-description = Raison : {$orderReason}
cargo-console-menu-order-row-button-approve = Approuver
cargo-console-menu-order-row-button-cancel = Annuler
cargo-console-menu-order-row-alerts-reason-absent = La raison n'est pas spécifiée
cargo-console-menu-order-row-alerts-requester-unknown = Inconnu
cargo-console-menu-populate-categories-all-text = Tout
cargo-console-menu-tab-title-orders = Commandes
cargo-console-menu-tab-title-funds = Transferts
cargo-console-menu-account-action-transfer-limit = Limite de transfert :
cargo-console-menu-account-action-transfer-limit-amount = ${$amount}
cargo-console-menu-account-action-transfer-limit-unlimited-notifier = [color=gold](Illimitée)[/color]
cargo-console-menu-account-action-select = [bold]Action du compte :[/bold]
cargo-console-menu-account-action-amount = [bold]Montant :[/bold] $
cargo-console-menu-account-action-button = Transférer
cargo-console-menu-toggle-account-lock-button = Basculer la limite de transfert
cargo-console-menu-account-action-option-withdraw = Retirer des espèces
cargo-console-menu-account-action-option-transfer = Transférer des fonds vers {$code}

# Orders
cargo-console-order-not-allowed = Accès non autorisé
cargo-console-station-not-found = Aucune station disponible
cargo-console-invalid-product = ID de produit invalide
cargo-console-too-many = Trop de commandes approuvées
cargo-console-snip-snip = Commande réduite à la capacité
cargo-console-insufficient-funds = Fonds insuffisants (nécessite {$cost})
cargo-console-unfulfilled = Pas de place pour exécuter la commande
cargo-console-trade-station = Envoyé à {$destination}
cargo-console-unlock-approved-order-broadcast = [bold]{$productName} x{$orderAmount}[/bold], d'un coût de [bold]{$cost}[/bold], a été approuvé par [bold]{$approver}[/bold]
cargo-console-fund-withdraw-broadcast = [bold]{$name} a retiré {$amount} spesos de {$name1} \[{$code1}\]
cargo-console-fund-transfer-broadcast = [bold]{$name} a transféré {$amount} spesos de {$name1} \[{$code1}\] vers {$name2} \[{$code2}\][/bold]
cargo-console-fund-transfer-user-unknown = Inconnu

# GoobStation - cooldown on Cargo Orders (specifically gamba)
cargo-console-cooldown-count = Impossible de commander plus d'un(e) {$product} à la fois.
cargo-console-cooldown-active = Les commandes de {$product} ne peuvent pas être passées avant {$timeCount} {$timeUnits} supplémentaires.

cargo-console-paper-reason-default = Aucune
cargo-console-paper-approver-default = Soi-même
cargo-console-paper-print-name = Commande #{$orderNumber}
cargo-console-paper-print-text = [head=2]Commande #{$orderNumber}[/head]
    {"[bold]Article :[/bold]"} {$itemName} (x{$orderQuantity})
    {"[bold]Demandé par :[/bold]"} {$requester}

    {"[head=3]Informations sur la commande[/head]"}
    {"[bold]Payeur[/bold] :"} {$account} [font="Monospace"]\[{$accountcode}\][/font]
    {"[bold]Approuvé par :[/bold]"} {$approver}
    {"[bold]Raison :[/bold]"} {$reason}

# Cargo shuttle console
cargo-shuttle-console-menu-title = Console de la navette de cargo
cargo-shuttle-console-station-unknown = Inconnue
cargo-shuttle-console-shuttle-not-found = Introuvable
cargo-shuttle-console-organics = Formes de vie organiques détectées sur la navette
cargo-no-shuttle = Aucune navette de cargo trouvée !

# Funding allocation console
cargo-funding-alloc-console-menu-title = Console d'allocation des fonds
cargo-funding-alloc-console-label-account = [bold]Compte[/bold]
cargo-funding-alloc-console-label-code = [bold] Code [/bold]
cargo-funding-alloc-console-label-balance = [bold] Solde [/bold]
cargo-funding-alloc-console-label-cut = [bold] Répartition des revenus (%) [/bold]

cargo-funding-alloc-console-label-primary-cut = Part du cargo sur les fonds provenant de sources hors coffre-fort (%) :
cargo-funding-alloc-console-label-lockbox-cut = Part du cargo sur les fonds provenant des ventes de coffres-forts (%) :

cargo-funding-alloc-console-label-help-non-adjustible = Le cargo reçoit {$percent}% des bénéfices des ventes hors coffre-fort. Le reste est réparti comme spécifié ci-dessous :
cargo-funding-alloc-console-label-help-adjustible = Les fonds restants des sources hors coffre-fort sont distribués comme spécifié ci-dessous :
cargo-funding-alloc-console-button-save = Enregistrer les modifications
cargo-funding-alloc-console-label-save-fail = [bold]Répartitions des revenus invalides ![/bold] [color=red]({$pos ->
    [1] +
    *[-1] -
}{$val}%)[/color]

# Slip template
cargo-acquisition-slip-body = [head=3]Détail de l'actif[/head]
    {"[bold]Produit :[/bold]"} {$product}
    {"[bold]Description :[/bold]"} {$description}
    {"[bold]Coût unitaire :[/bold]"} ${$unit}
    {"[bold]Quantité :[/bold]"} {$amount}
    {"[bold]Coût :[/bold]"} ${$cost}

    {"[head=3]Détail de l'achat[/head]"}
    {"[bold]Commandé par :[/bold]"} {$orderer}
    {"[bold]Raison :[/bold]"} {$reason}