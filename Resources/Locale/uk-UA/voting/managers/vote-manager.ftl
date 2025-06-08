# Displayed as initiator of vote when no user creates the vote
ui-vote-initiator-server = Сервер

## Default.Votes

ui-vote-restart-title = Рестарт
ui-vote-restart-succeeded = Голосування за перезапуск успішне.
ui-vote-restart-failed = Голосування за перезапуск провалилось (потрібно { TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Голосування за перезапуск провалено: Для ініціювання голосування за перезапуск потрібно мінімум { $ghostPlayerRequirement }% гравців-привидів. Наразі недостатньо гравців-привидів.
ui-vote-restart-yes = Так
ui-vote-restart-no = Ні
ui-vote-restart-abstain = Утриматись
ui-vote-restart-fail-min-round-duration = Голосування за перезапуск провалено: Поточний раунд триває менше { $minRoundDuration } хвилин. Почекайте трохи.

ui-vote-gamemode-title = Наступний ігровий режим
ui-vote-gamemode-tie = Нічия у голосуванні! Вибираємо... { $picked }
ui-vote-gamemode-win = { $winner } виграв голосування режиму гри!

ui-vote-map-title = Наступна мапа
ui-vote-map-tie = Нічия у голосувані! Вибираємо... { $picked }
ui-vote-map-win = { $winner } виграв голосування за мапу!
ui-vote-map-notlobby = Голосування за мапу дійсне лише в лобі!
ui-vote-map-notlobby-time = Голосування за мапи дійсне лише в передраундовому лобі з { $time } залишилось!

ui-vote-votekick-unknown-initiator = Гравець
ui-vote-votekick-unknown-target = Невідомий гравець
ui-vote-votekick-title = { $initiator } розпочав голосування за виключення користувача: { $targetEntity }. Причина: { $reason }
ui-vote-votekick-yes = Так
ui-vote-votekick-no = Ні
ui-vote-votekick-abstain = Утриматися
ui-vote-votekick-success = Голосування за виключення { $target } успішне. Причина: { $reason }
ui-vote-votekick-failure = Голосування за виключення { $target } провалено. Причина: { $reason }
ui-vote-votekick-not-enough-eligible = Недостатньо гравців онлайн для початку голосування: { $voters }/{ $requirement }
ui-vote-votekick-server-cancelled = Голосування за виключення { $target } було скасовано сервером.