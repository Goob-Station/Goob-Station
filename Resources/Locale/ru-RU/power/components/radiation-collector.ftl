# SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

power-radiation-collector-gas-tank-missing = Выемка для баллона с плазмой [color=darkred]пустует[/color].
power-radiation-collector-gas-tank-present =
    Выемка для баллона с плазмой [color=darkgreen]заполнена[/color] и индикатор баллона находится на отметке [color={ $fullness ->
       *[0] red]пусто
        [1] red]мало
        [2] yellow]заполнено наполовину
        [3] lime]заполнено
    }[/color].
power-radiation-collector-enabled =
    Находится в режиме [color={ $state ->
        [true] darkgreen]вкл
       *[false] darkred]выкл
    }[/color].
