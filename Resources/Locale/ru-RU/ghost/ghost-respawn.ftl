ghost-respawn-time-left = До возможности вернуться в раунд { $time }
    { $time ->
        [one] минута
        [few] минуты
       *[other] минут
    }
ghost-respawn-minutes-left = До возможности вернуться в раунд { $time } минут
    { $time ->
        [one] минута
        [few] минуты
       *[other] минут
    }
ghost-respawn-success = Возрождение успешно
ghost-respawn-seconds-left = До возможности вернуться в раунд { $time } секунд
ghost-respawn-max-players = Функция недоступна, игроков на сервере должно быть меньше { $players }.
ghost-respawn-window-title = Правила возвращения в раунд
ghost-respawn-window-rules-footer = Пользуясь это функцией, вы [color=#ff7700]обязуетесь[/color] [color=#ff0000]не переносить[/color] знания своего прошлого персонажа в нового. За нарушение пункта, указанного здесь, следует [color=#ff0000]бан в размере от 3-ех дней[/color].
ghost-respawn-same-character = Нельзя заходить в раунд за того же персонажа. Поменяйте его в настройках персонажей.

ghost-respawn-log-character-almost-same = Игрок { $player } { $try ->
    [true] зашёл
    *[false] попытался зайти
} в раунд после возвращения в лобби с похожим именем. Прошлое имя: { $oldName }, текущее: { $newName }.
ghost-respawn-log-return-to-lobby = { $userName } вернулся в лобби.
