# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Localization used for the list verbs command.
# Mostly help + error messages.

list-verbs-command-description = Lists all verbs that a player can use on a given entity.
list-verbs-command-help = listverbs <playerUid | "self"> <targetUid>

list-verbs-command-invalid-args = listverbs takes 2 arguments.

list-verbs-command-invalid-player-uid = Player uid could not be parsed, or "self" was not passed.
list-verbs-command-invalid-target-uid = Target uid could not be parsed.

list-verbs-command-invalid-player-entity = Player uid given does not correspond to a valid entity.
list-verbs-command-invalid-target-entity = Target uid given does not correspond to a valid entity.

list-verbs-verb-listing = { $type }: { $verb }
