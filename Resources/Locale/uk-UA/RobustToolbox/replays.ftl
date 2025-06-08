# Playback Commands

cmd-replay-play-desc = Відновити відтворення повтору.
cmd-replay-play-help = replay_play

cmd-replay-pause-desc = Призупинити відтворення повтору
cmd-replay-pause-help = replay_pause

cmd-replay-toggle-desc = Відновити або призупинити відтворення повтору.
cmd-replay-toggle-help = replay_toggle

cmd-replay-stop-desc = Зупинити та вивантажити повтор.
cmd-replay-stop-help = replay_stop

cmd-replay-load-desc = Завантажити та запустити повтор.
cmd-replay-load-help = replay_load <папка_повтору>
cmd-replay-load-hint = Папка повтору

cmd-replay-skip-desc = Перемотати час вперед або назад.
cmd-replay-skip-help = replay_skip <тік або проміжок_часу>
cmd-replay-skip-hint = Тіки або проміжок часу (ГГ:ХХ:СС).

cmd-replay-set-time-desc = Перейти вперед або назад до певного моменту часу.
cmd-replay-set-time-help = replay_set <тік або час>
cmd-replay-set-time-hint = Тік або проміжок часу (ГГ:ХХ:СС), починаючи з

cmd-replay-error-time = "{$time}" не є цілим числом або проміжком часу.
cmd-replay-error-args = Неправильна кількість аргументів.
cmd-replay-error-no-replay = Наразі повтор не відтворюється.
cmd-replay-error-already-loaded = Повтор вже завантажено.
cmd-replay-error-run-level = Ви не можете завантажити повтор, будучи підключеним до сервера.

# Команди запису

cmd-replay-recording-start-desc = Починає запис повтору, за бажанням з обмеженням часу.
cmd-replay-recording-start-help = Використання: replay_recording_start [назва] [перезаписати] [обмеження_часу]
cmd-replay-recording-start-success = Розпочато запис повтору.
cmd-replay-recording-start-already-recording = Запис повтору вже ведеться.
cmd-replay-recording-start-error = Сталася помилка під час спроби розпочати запис.
cmd-replay-recording-start-hint-time = [обмеження часу (хвилини)]
cmd-replay-recording-start-hint-name = [назва]
cmd-replay-recording-start-hint-overwrite = [overwrite (bool)]

cmd-replay-recording-stop-desc = Зупиняє запис повтору.
cmd-replay-recording-stop-help = Використання: replay_recording_stop
cmd-replay-recording-stop-success = Запис повтору зупинено.
cmd-replay-recording-stop-not-recording = Наразі запис повтору не ведеться.

cmd-replay-recording-stats-desc = Відображає інформацію про поточний запис повтору.
cmd-replay-recording-stats-help = Використання: replay_recording_stats
cmd-replay-recording-stats-result = Тривалість: {$time} хв, Тіки: {$ticks}, Розмір: {$size} МБ, швидкість: {$rate} МБ/хв.


# Інтерфейс керування часом
replay-time-box-scrubbing-label = Динамічне перемотування
replay-time-box-replay-time-label = Час запису: {$current} / {$end}  ({$percentage}%)
replay-time-box-server-time-label = Час сервера: {$current} / {$end}
replay-time-box-index-label = Індекс: {$current} / {$total}
replay-time-box-tick-label = Тік: {$current} / {$total}
