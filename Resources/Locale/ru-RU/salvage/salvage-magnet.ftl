# SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 CrigCrag <137215465+CrigCrag@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Zadeon <loldude9000@gmail.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

salvage-system-announcement-losing = Магнит больше не может удерживать обломок. Оставшееся время удержания: { $timeLeft } секунд.
salvage-system-announcement-spawn-debris-disintegrated = Обломок дезинтегрировал во время орбитального перемещения.
salvage-system-announcement-spawn-no-debris-available = Нет обломков, которые можно притянуть магнитом.
salvage-system-announcement-arrived = Обломок был притянут для утилизации. Расчётное время удержания: { $timeLeft } секунд.
salvage-asteroid-name = Астероид

salvage-magnet-window-title = Магнит обломков
salvage-expedition-window-progression = Прогресс

salvage-magnet-resources =
    { $resource ->
        [OreIron] Железо
        [OreCoal] Уголь
        [OreQuartz] Кварц
        [OreSalt] Соль
        [OreGold] Золото
        [OreDiamond] Алмазы
        [OreSilver] Серебро
        [OrePlasma] Плазма
        [OreUranium] Уран
        [OreArtifactFragment] Фрагменты артефактов
        [OreBananium] Бананиум
       *[other] { $resource }
    }

salvage-magnet-resources-count =
    { $count ->
        [1] (Мало)
        [2] (Средне)
        [3] (Средне)
        [4] (Много)
        [5] (Много)
       *[other] (Изобилие)
    }

# Debris
salvage-magnet-debris-ChunkDebris = Космический обломок

# Asteroids
dungeon-config-proto-BlobAsteroid = Астероидный массив
dungeon-config-proto-ClusterAsteroid = Астероидный кластер
dungeon-config-proto-SpindlyAsteroid = Астероидная спираль
dungeon-config-proto-SwissCheeseAsteroid = Фрагменты астероидов

# Wrecks
salvage-map-wreck = Обломок для утилизации
salvage-map-wreck-desc-size = Размер:
salvage-map-wreck-size-small = [color=lime]Малый[/color]
salvage-map-wreck-size-medium = [color=cornflowerblue]Средний[/color]
salvage-map-wreck-size-large = [color=orchid]Большой[/color]
