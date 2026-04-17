# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-bql_select-desc = Показать результаты BQL запроса в клиентском окне
cmd-bql_select-help =
    Использование: bql_select <bql запрос>
    Открытое окно позволяет телепортироваться к результирующим сущностям или просматривать их переменные.

cmd-bql_select-err-server-shell = Не может быть выполнено из серверной оболочки
cmd-bql_select-err-rest = Предупреждение: неиспользуемая часть после BQL запроса: "{ $rest }"

ui-bql-results-title = Результаты BQL
ui-bql-results-vv = VV
ui-bql-results-tp = ТП
ui-bql-results-vv-tooltip = Просмотреть переменные сущности
ui-bql-results-tp-tooltip = Телепортироваться к сущности
ui-bql-results-status = { $count } сущностей
