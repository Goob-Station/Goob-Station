# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 veprolet <68151557+veprolet@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Solution transfer component

comp-solution-transfer-fill-normal = Vous remplissez {THE($target)} avec {$amount}u depuis {THE($owner)}.
comp-solution-transfer-fill-fully = Vous remplissez {THE($target)} à ras bord avec {$amount}u depuis {THE($owner)}.
comp-solution-transfer-transfer-solution = Vous transférez {$amount}u vers {THE($target)}.

## Displayed when trying to transfer to a solution, but either the giver is empty or the taker is full
comp-solution-transfer-is-empty = {CAPITALIZE(THE($target))} est vide !
comp-solution-transfer-is-full = {CAPITALIZE(THE($target))} est plein !

## Displayed in change transfer amount verb's name
comp-solution-transfer-verb-custom-amount = Personnalisé
comp-solution-transfer-verb-amount = {$amount}u
comp-solution-transfer-verb-toggle = Basculer vers {$amount}u

## Displayed after you successfully change a solution's amount using the BUI
comp-solution-transfer-set-amount = Quantité de transfert définie à {$amount}u.
comp-solution-transfer-set-amount-max = Max : {$amount}u
comp-solution-transfer-set-amount-min = Min : {$amount}u
