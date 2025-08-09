# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 PrPleGoo <PrPleGoo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

agent-id-new = { $number ->
    [0] Didn't gain any new accesses from {THE($card)}.
    [one] Gained one new access from {THE($card)}.
   *[other] Gained {$number} new accesses from {THE($card)}.
}

agent-id-card-current-name = Name:
agent-id-card-current-job = Job:
agent-id-card-job-icon-label = Job icon:
agent-id-menu-title = Agent ID Card
