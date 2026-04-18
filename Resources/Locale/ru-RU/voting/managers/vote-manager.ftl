# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Mervill <mervills.email@gmail.com>
# SPDX-FileCopyrightText: 2023 alexkar598 <25136265+alexkar598@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Repo <47093363+Titian3@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Displayed as initiator of vote when no user creates the vote
ui-vote-initiator-server = Сервер

## Default.Votes

ui-vote-restart-title = Перезапуск раунда
ui-vote-restart-succeeded = Голосование о перезапуске раунда успешно.
ui-vote-restart-failed = Голосование о перезапуске раунда отклонено (требуется { TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Голосование о перезапуске раунда отклонено: Минимум { $ghostPlayerRequirement }% игроков должно быть призраками чтобы запустить голосование о перезапуске. В данный момент игроков-призраков недостаточно.
ui-vote-restart-yes = Да
ui-vote-restart-no = Нет
ui-vote-restart-abstain = Воздерживаюсь

ui-vote-gamemode-title = Следующий режим игры
ui-vote-gamemode-tie = Ничья в голосовании за игровой режим! Выбирается... { $picked }
ui-vote-gamemode-win = { $winner } победил в голосовании за игровой режим!

ui-vote-map-title = Следующая карта
ui-vote-map-tie = Ничья при голосовании за карту! Выбирается... { $picked }
ui-vote-map-win = { $winner } выиграла голосование о выборе карты!
ui-vote-map-notlobby = Голосование о выборе карты действует только в предраундовом лобби!
ui-vote-map-notlobby-time = Голосование о выборе карты действует только в предраундовом лобби, когда осталось { $time }!


# Votekick votes
ui-vote-votekick-unknown-initiator = Игрок
ui-vote-votekick-unknown-target = Неизвестный игрок
ui-vote-votekick-title = { $initiator } начал голосование за кик пользователя: { $targetEntity }. Причина: { $reason }
ui-vote-votekick-yes = Да
ui-vote-votekick-no = Нет
ui-vote-votekick-abstain = Воздержаться
ui-vote-votekick-success = Голосование за кик { $target } прошло успешно. Причина кика: { $reason }
ui-vote-votekick-failure = Голосование за кик { $target } провалилось. Причина кика: { $reason }
ui-vote-votekick-not-enough-eligible = Недостаточное количество подходящих голосующих онлайн для начала голосования: { $voters }/{ $requirement }
ui-vote-votekick-server-cancelled = Голосование за кик { $target } отменено сервером.
