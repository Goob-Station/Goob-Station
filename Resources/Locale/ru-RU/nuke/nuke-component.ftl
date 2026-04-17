# SPDX-FileCopyrightText: 2021 Alexander Evgrashin <evgrashin.adl@gmail.com>
# SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
# SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Errant <35878406+Errant-4@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 lapatison <100279397+lapatison@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

nuke-component-cant-anchor-floor = Крепёжным болтам не удаётся закрепиться в полу!
nuke-component-cant-anchor-toggle = Для переключения крепёжных болтов требуется диск ядерной аутентификации!
nuke-component-announcement-sender = Ядерная боеголовка
nuke-component-announcement-armed = Внимание! Механизм самоуничтожения станции был активирован { $location }. До детонации { $time } секунд.
nuke-component-announcement-unarmed = Механизм самоуничтожение станции деактивирован! Хорошего дня!
nuke-component-announcement-send-codes = Внимание! Запрошенные коды самоуничтожения были отправлены на факс капитана.
nuke-component-doafter-warning = Вы начинаете перебирать провода и кнопки, в попытке обезвредить ядерную бомбу. Это может занять некоторое время.

nuke-disk-component-microwave = The disk sparks and fizzles a bit, but seems mostly unharmed?

# Nuke UI
nuke-user-interface-title = Ядерная боеголовка
nuke-user-interface-arm-button = ВЗВЕСТИ
nuke-user-interface-disarm-button = ОБЕЗВРЕДИТЬ
nuke-user-interface-anchor-button = ЗАКРЕПИТЬ
nuke-user-interface-eject-button = ИЗВЛЕЧЬ

## Upper status
nuke-user-interface-first-status-device-locked = УСТРОЙСТВО ЗАБЛОКИРОВАНО
nuke-user-interface-first-status-input-code = ВВЕДИТЕ КОД
nuke-user-interface-first-status-input-time = ВВЕДИТЕ ВРЕМЯ
nuke-user-interface-first-status-device-ready = УСТРОЙСТВО ГОТОВО
nuke-user-interface-first-status-device-armed = УСТРОЙСТВО ВЗВЕДЕНО
nuke-user-interface-first-status-device-cooldown = ДЕАКТИВИРОВАНО
nuke-user-interface-status-error = ОШИБКА

## Lower status
nuke-user-interface-second-status-await-disk = ОЖИДАНИЕ ДИСКА
nuke-user-interface-second-status-time = ВРЕМЯ: { $time }
nuke-user-interface-second-status-current-code = КОД: { $code }
nuke-user-interface-second-status-cooldown-time = ОЖИДАНИЕ: { $time }

## Nuke labels
nuke-label-nanotrasen = NT-{ $serial }

# do you even need this one? It's more funnier to say that
# the Syndicate stole a NT nuke
nuke-label-syndicate = SYN-{ $serial }

# Codes
nuke-codes-message = [color=red]СОВЕРШЕННО СЕКРЕТНО![/color]
nuke-codes-list = Код { $name }: { $code }
nuke-codes-fax-paper-name = коды ядерной аутентификации

# Nuke disk slot
nuke-slot-component-slot-name-disk = Диск

## Examine
nuke-examine-armed = Эй, а почему эта [color=red]красная лампочка[/color] мигает?
nuke-examine-exploding = Ага... Похоже, уже слишком поздно, приятель.
