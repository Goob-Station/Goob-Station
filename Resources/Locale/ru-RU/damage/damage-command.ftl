# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Damage command loc.

damage-command-description = Добавить или убрать урон сущности.
damage-command-help = Использование: { $command } <type/group> <amount> [ignoreResistances] [uid]

damage-command-arg-type = <damage type or group>
damage-command-arg-quantity = [quantity]
damage-command-arg-target = [target euid]

damage-command-error-type = { $arg } неправильная группа или тип урона.
damage-command-error-euid = { $arg } неправильный UID сущности.
damage-command-error-quantity = { $arg } неправильное количество.
damage-command-error-bool = { $arg } неправильное логическое значение.
damage-command-error-player = Нет сущности, привязанной к сессии. Вы должны указать UID цели.
damage-command-error-args = Неправильное количество аргументов.
