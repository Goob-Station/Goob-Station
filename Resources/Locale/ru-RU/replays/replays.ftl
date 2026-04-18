# SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Loading Screen

replay-loading = Загрузка ({ $cur }/{ $total })
replay-loading-reading = Чтение файлов
replay-loading-processing = Обработка файлов
replay-loading-spawning = Спавн сущностей
replay-loading-initializing = Инициализация сущностей
replay-loading-starting = Запуск сущностей
replay-loading-failed =
    Не удалось загрузить повтор. Ошибка:
    { $reason }
replay-loading-retry = Попробовать загрузить с большей допустимостью исключений - МОЖЕТ ВЫЗВАТЬ БАГИ!
replay-loading-cancel = Отмена

# Main Menu
replay-menu-subtext = Повторы
replay-menu-load = Загрузить выбранный повтор
replay-menu-select = Выбрать повтор
replay-menu-open = Открыть папку повторов
replay-menu-none = Повторы не найдены.

# Main Menu Info Box
replay-info-title = Информация о повторе
replay-info-none-selected = Повтор не выбран
replay-info-invalid = [color=red]Выбран неверный повтор[/color]
replay-info-info =
    { "[" }color=gray]Выбрано:[/color]  { $name } ({ $file })
    { "[" }color=gray]Время:[/color]   { $time }
    { "[" }color=gray]ID раунда:[/color]   { $roundId }
    { "[" }color=gray]Продолжительность:[/color]   { $duration }
    { "[" }color=gray]ForkId:[/color]   { $forkId }
    { "[" }color=gray]Версия:[/color]   { $version }
    { "[" }color=gray]Движок:[/color]   { $engVersion }
    { "[" }color=gray]Type Hash:[/color]   { $hash }
    { "[" }color=gray]Comp Hash:[/color]   { $compHash }

# Replay selection window
replay-menu-select-title = Выбрать повтор

# Replay related verbs
replay-verb-spectate = Наблюдать

# command
cmd-replay-spectate-help = replay_spectate [сущность (опционально)]
cmd-replay-spectate-desc = Прикрепляет или открепляет локального игрока к заданному uid сущности.
cmd-replay-spectate-hint = Опциональный EntityUid

cmd-replay-toggleui-desc = Переключение пользовательского интерфейса управления воспроизведением.
