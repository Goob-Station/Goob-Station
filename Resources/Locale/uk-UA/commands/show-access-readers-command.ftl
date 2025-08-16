# SPDX-FileCopyrightText: 2022 Elijahrane <60792108+Elijahrane@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-showaccessreaders-desc = Перемикає показ дозволів зчитувачів доступу на карті
cmd-showaccessreaders-help = Інформація про накладення:
    -Вимкнено | Зчитувач доступу вимкнено
    +Без обмежень | Зчитувач доступу не має обмежень
    +Набір [Індекс]: [Назва тегу]| Тег у наборі доступу (для доступу потрібні всі теги в наборі)
    +Ключ [StationUid]: [StationRecordKeyId] | Дозволений StationRecordKey
    -Тег [Назва тегу] | Недозволений тег (має пріоритет над іншими дозволами)
cmd-showaccessreaders-status = Накладення налагодження зчитувачів доступу встановлено на {$status}.
