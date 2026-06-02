# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 PrPleGoo <PrPleGoo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

agent-id-new = { $number ->
    [0] Aucun nouvel accès obtenu depuis {THE($card)}.
    [one] Un nouvel accès obtenu depuis {THE($card)}.
   *[other] {$number} nouveaux accès obtenus depuis {THE($card)}.
}

agent-id-card-current-name = Nom :
agent-id-card-current-job = Poste :
agent-id-card-job-icon-label = Icône de poste :
agent-id-menu-title = Carte d'identité d'agent
