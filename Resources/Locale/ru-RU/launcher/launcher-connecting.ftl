# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Repo <47093363+Titian3@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Vigers Ray <60344369+VigersRay@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Connecting dialog when you start up the game

connecting-title = Space Station 14
connecting-exit = Выйти
connecting-retry = Повторить
connecting-reconnect = Переподключиться
connecting-copy = Скопировать сообщение
connecting-redial = Перезапустить
connecting-redial-wait = Пожалуйста подождите: { TOSTRING($time, "G3") }
connecting-in-progress = Подключение к серверу...
connecting-disconnected = Отключён от сервера:
connecting-tip = Не умирай!
connecting-window-tip = Совет { $numberTip }
connecting-version = версия 0.1
connecting-fail-reason =
    Не удалось подключиться к серверу:
    { $reason }
connecting-state-NotConnecting = Не подключён
connecting-state-ResolvingHost = Определение хоста
connecting-state-EstablishingConnection = Установка соединения
connecting-state-Handshake = Handshake
connecting-state-Connected = Подключён
