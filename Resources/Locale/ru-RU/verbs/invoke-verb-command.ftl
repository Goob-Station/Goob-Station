# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Localization used for the invoke verb command.
# Mostly help + error messages.

invoke-verb-command-description = Вызывает verb с заданным именем на сущности, с сущностью игрока
invoke-verb-command-help = invokeverb <playerUid | "self"> <targetUid> <verbName | "interaction" | "activation" | "alternative">

invoke-verb-command-invalid-args = invokeverb принимает 2 аргумента.

invoke-verb-command-invalid-player-uid = uid игрока не может быть проанализирован, или "self" не было пройдено.
invoke-verb-command-invalid-target-uid = Целевой uid не может быть проанализирован.

invoke-verb-command-invalid-player-entity = Указанный uid игрока не соответствует действительной сущности.
invoke-verb-command-invalid-target-entity = Указанный целевой uid не соответствует действительной сущности.

invoke-verb-command-success = Вызывает verb '{ $verb }' на { $target } с { $player } в качестве пользователя.

invoke-verb-command-verb-not-found = Не удалось найти verb { $verb } на { $target }.
