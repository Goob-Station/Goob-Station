## Strings for the "grant_connect_bypass" command.

cmd-grant_connect_bypass-desc = Временно позволяет пользователю обходить регулярные проверки соединения.
cmd-grant_connect_bypass-help =
    Использование: grant_connect_bypass <пользователь> [длительность в минутах]
    Временно предоставляет пользователю возможность обходить ограничения на обычные соединения.
    Обход применяется только к данному игровому серверу и истекает через (по умолчанию) 1 час.
    Пользователь сможет присоединиться к игре независимо от наличия белого списка, бункера паники или ограничения количества игроков.
cmd-grant_connect_bypass-arg-user = <пользователь>
cmd-grant_connect_bypass-arg-duration = [длительность в минутах]
cmd-grant_connect_bypass-invalid-args = Ожидается 1 или 2 аргумента
cmd-grant_connect_bypass-unknown-user = Невозможно найти пользователя '{ $user }'
cmd-grant_connect_bypass-invalid-duration = Недопустимая продолжительность '{ $duration }'
cmd-grant_connect_bypass-success = Успешно добавлен обход для пользователя '{ $user }'
