# SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

robotics-console-window-title = Роботехническая консоль
robotics-console-no-cyborgs = Борги отсутствуют!

robotics-console-select-cyborg = Выберите борга из списка выше.
robotics-console-model = [color=gray]Модель:[/color] { $name }
# name is not formatted to prevent players trolling
robotics-console-designation = [color=gray]Назначение:[/color]
robotics-console-battery = [color=gray]Заряд батареи:[/color] [color={ $color }]{ $charge }[/color]%
robotics-console-modules = [color=gray]Установленные модули:[/color] { $count }
robotics-console-brain = [color=gray]Мозг установлен:[/color] [color={ $brain ->
        [true] green]Да
       *[false] red]Нет
    }[/color]

robotics-console-locked-message = Управление заблокировано, проведите ID-картой.
robotics-console-disable = Отключить
robotics-console-destroy = Уничтожить

robotics-console-cyborg-destroying = Запущен процесс дистанционного уничтожения { $name }!
