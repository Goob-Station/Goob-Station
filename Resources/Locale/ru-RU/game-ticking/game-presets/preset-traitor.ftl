# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Morbo <exstrominer@gmail.com>
# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Interrobang01 <113810873+Interrobang01@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ZeroDayDaemon <60460608+ZeroDayDaemon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Arkanic <50847107+Arkanic@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Traitor

traitor-round-end-codewords = Кодовыми словами были: [color=White]{ $codewords }[/color].
traitor-round-end-agent-name = предатель

objective-issuer-syndicate = [color=crimson]Синдикат[/color]
objective-issuer-unknown = Неизвестно

# Shown at the end of a round of Traitor

traitor-title = Предатели
traitor-description = Среди нас есть предатели...
traitor-not-enough-ready-players = Недостаточно игроков готовы к игре! Из { $minimumPlayers } необходимых игроков готовы { $readyPlayersCount }. Нельзя запустить пресет Предатели.
traitor-no-one-ready = Нет готовых игроков! Нельзя запустить пресет Предатели.

## TraitorDeathMatch
traitor-death-match-title = Бой насмерть предателей
traitor-death-match-description = Все — предатели. Все хотят смерти друг друга.
traitor-death-match-station-is-too-unsafe-announcement = На станции слишком опасно, чтобы продолжать. У вас есть одна минута.
traitor-death-match-end-round-description-first-line = КПК были восстановлены...
traitor-death-match-end-round-description-entry = КПК { $originalName }, с { $tcBalance } ТК

## TraitorRole

# TraitorRole
traitor-role-greeting =
    Вы - агент организации { $corporation } на задании [color = darkred]Синдиката.[/color].
    Ваши цели и кодовые слова перечислены в меню персонажа.
    Воспользуйтесь своим аплинком, чтобы приобрести всё необходимое для выполнения работы.
    Смерть NanoTrasen!
traitor-role-codewords =
    Кодовые слова следующие: [color = lightgray]
    { $codewords }.[/color]
    Кодовые слова можно использовать в обычном разговоре, чтобы незаметно идентифицировать себя для других агентов Синдиката.
    Прислушивайтесь к ним и храните их в тайне.
traitor-role-uplink-code =
    Установите рингтон Вашего КПК на [color = lightgray]{ $code }[/color] чтобы заблокировать или разблокировать аплинк.
    Не забудьте заблокировать его и сменить код, иначе кто угодно из экипажа станции сможет открыть аплинк!
traitor-role-uplink-pen-code =
    Прокрутите ручку на комбинацию [color = lightgray]{ $code }[/color], чтобы разблокировать аплинк.
    Градусы обозначают углы поворота. Аплинк автоматически блокируется при закрытии.
traitor-role-uplink-implant =
    Ваш имплант аплинк активирован, воспользуйтесь им из хотбара.
    Аплинк надежно защищён, пока кто-нибудь не извлечёт его из вашего тела.

# don't need all the flavour text for character menu
traitor-role-codewords-short =
    Кодовые слова:
    { $codewords }.

traitor-role-uplink-code-short = Ваш код аплинка: { $code }. Установите его в качестве рингтона КПК для доступа к аплинку.
traitor-role-uplink-pen-code-short = Комбинация от Аплинка вашей ручки: { $code }. Прокрутите ручку для разблокировки. Блокируется при закрытии.
traitor-role-uplink-implant-short = Ваш аплинк был имплантирован. Воспользуйтесь им из хотбара.

traitor-role-moreinfo = Найдите больше информации о своей роли в меню персонажа.

traitor-role-nouplink = У вас нет аплинка Синдиката. Действуйте обдуманно.

traitor-role-allegiances = Ваша приверженность:

traitor-role-notes = Заметки от вашего нанимателя:
