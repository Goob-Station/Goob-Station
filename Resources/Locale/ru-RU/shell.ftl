# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morber <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
# SPDX-FileCopyrightText: 2023 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### for technical and/or system messages

## General

shell-command-success = Команда выполнена.
shell-invalid-command = Неверная команда.
shell-invalid-command-specific = Неверная команда { $commandName }.
shell-cannot-run-command-from-server = Вы не можете выполнить эту команду с сервера.
shell-only-players-can-run-this-command = Только игроки могут выполнять эту команду.
shell-must-be-attached-to-entity = Для выполнения этой команды вы должны быть прикреплены к сущности.
shell-must-have-body = У вас должно быть тело для использования этой команды.

## Arguments

shell-need-exactly-one-argument = Нужен ровно один аргумент.
shell-wrong-arguments-number-need-specific =
    Нужно { $properAmount } { $properAmount ->
        [one] аргумент
        [few] аргумента
       *[other] аргументов
    }, было { $currentAmount } { $currentAmount ->
        [one] аргумент
        [few] аргумента
       *[other] аргументов
    }.
shell-argument-must-be-number = Аргумент должен быть числом.
shell-argument-must-be-boolean = Аргумент должен быть boolean.
shell-wrong-arguments-number = Неправильное количество аргументов.
shell-need-between-arguments = Нужно от { $lower } до { $upper } аргументов!
shell-need-minimum-arguments = Нужно не менее { $minimum } аргументов!
shell-need-minimum-one-argument = Нужен хотя бы один аргумент!
shell-need-exactly-zero-arguments = Эта команда принимает ноль аргументов.

shell-argument-uid = EntityUid

## Guards

shell-missing-required-permission = Вам нужен { $perm } для этой команды!
shell-entity-is-not-mob = Целевая сущность не является мобом!
shell-invalid-entity-id = Недопустимый ID сущности.
shell-invalid-grid-id = Недопустимый ID сетки.
shell-invalid-map-id = Недопустимый ID карты.
shell-invalid-entity-uid = { $uid } не является допустимым идентификатором uid.
shell-invalid-bool = Неверный boolean.
shell-entity-uid-must-be-number = EntityUid должен быть числом.
shell-could-not-find-entity = Не удалось найти сущность { $entity }.
shell-could-not-find-entity-with-uid = Не удалось найти сущность с uid { $uid }.
shell-entity-with-uid-lacks-component = Сущность с uid { $uid } не имеет компонента { $componentName }.
shell-entity-target-lacks-component = Целевая сущность не имеет компонента { $componentName }
shell-invalid-color-hex = Недопустимый HEX-цвет!
shell-target-player-does-not-exist = Целевой игрок не существует!
shell-target-entity-does-not-have-message = Целевая сущность не имеет { $missing }!
shell-timespan-minutes-must-be-correct = { $span } не является допустимым промежутком времени в минутах.
shell-argument-must-be-prototype = Аргумент { $index } должен быть ${ prototypeName }!
shell-argument-number-must-be-between = Аргумент { $index } должен быть числом от { $lower } до { $upper }!
shell-argument-station-id-invalid = Аргумент { $index } должен быть валидным station id!
shell-argument-map-id-invalid = Аргумент { $index } должен быть валидным map id!
shell-argument-number-invalid = Аргумент { $index } должен быть валидным числом!

# Hints
shell-argument-username-hint = <username>
shell-argument-username-optional-hint = [username]
