# SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nim <128169402+Nimfar11@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 deathride58 <deathride58@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Phrases used for where central command got this information.
random-sentience-event-data-1 = сканирование сенсорами дальнего действия
random-sentience-event-data-2 = наши сложные статистические модели вероятности
random-sentience-event-data-3 = наше всемогущество
random-sentience-event-data-4 = коммуникационный трафик с вашей станции
random-sentience-event-data-5 = обнаруженные нами энергетические всплески
random-sentience-event-data-6 = [УДАЛЕНО]

## Phrases used to describe the level of intelligence, though it doesn't actually affect anything.
random-sentience-event-strength-1 = человека
random-sentience-event-strength-2 = обезьяны
random-sentience-event-strength-3 = среднего
random-sentience-event-strength-4 = службы безопасности
random-sentience-event-strength-5 = командования
random-sentience-event-strength-6 = клоуна
random-sentience-event-strength-7 = низкого
random-sentience-event-strength-8 = ИИ

## Announcement text

station-event-random-sentience-announcement =
    Опираясь на { $data }, стало известно что некоторые { $amount ->
        [1] { $kind1 }
        [2] { $kind1 } и { $kind2 }
        [3] { $kind1 }, { $kind2 }, и { $kind3 }
       *[other] { $kind1 }, { $kind2 }, { $kind3 }, и т.д.
    } обрели интеллект уровня { $strength }, а также способность к общению.

## Ghost role description

station-event-random-sentience-role-description = Вы разумный { $name }, оживший благодаря космической магии.

# Flavors
station-event-random-sentience-flavor-mechanical = механизмы
station-event-random-sentience-flavor-organic = органики
station-event-random-sentience-flavor-corgi = корги
station-event-random-sentience-flavor-primate = приматы
station-event-random-sentience-flavor-kobold = кобольды
station-event-random-sentience-flavor-slime = слаймы
station-event-random-sentience-flavor-inanimate = неодушевлённые предметы
station-event-random-sentience-flavor-scurret = скюррет
