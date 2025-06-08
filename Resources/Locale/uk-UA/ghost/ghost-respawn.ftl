ghost-respawn-time-left = Перед респавном потрібно зачекати { $time } 
    { $time ->
        [one] хвилина
       *[other] хвилини
    }
ghost-respawn-max-players = Функція недоступна, на сервері має бути менше гравців { $players }.
ghost-respawn-window-title = Правила повернення до раунду
ghost-respawn-window-rules-footer = Використовуючи цю функцію, ви [color=#ff7700]погоджуєтеся[/color] [color=#ff0000]не переносити[/color] знання свого минулого персонажа на нового. За порушення пункту, зазначеного тут, [color=#ff0000]може трапитись бан на термін від одного дня[/color].
ghost-respawn-same-character = Ви не можете увійти в раунд тим же персонажем. Змініть персонажа в меню налаштування персонажів.

ghost-respawn-log-character-almost-same = Гравець { $player } { $try ->
    [true] приєднався
    *[false] спробував приєднатися
} у раунд після респауну зі схожим іменем. Минуле ім'я: { $oldName }, поточне: { $newName }.
ghost-respawn-log-return-to-lobby = { $userName } повернувся у лоббі.
ghost-respawn-minutes-left = Будь ласка, зачекайте {$time} {$time ->
    [one] хвилину
   *[other] хвилин
}, перш ніж намагатися відродитися.
ghost-respawn-seconds-left = Будь ласка, зачекайте {$time} {$time ->
    [one] секунду
   *[other] секунд
}, перш ніж намагатися відродитися.