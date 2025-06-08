# Playback Commands

cmd-replay-play-desc = Відновити повторне відтворення.
cmd-replay-play-help = replay_play

cmd-replay-pause-desc = Призупинити відтворення відтворення
cmd-replay-pause-help = replay_pause

cmd-replay-toggle-desc = Відновити або призупинити відтворення.
cmd-replay-toggle-help = replay_toggle

cmd-replay-stop-desc = Зупинка і вивантаження повтору.
cmd-replay-stop-help = replay_stop

cmd-replay-load-desc = Завантажте та запустіть відтворення.
cmd-replay-load-help = replay_load <папка відтворення>
cmd-replay-load-hint = Папка відтворення

cmd-replay-skip-desc = Переходьте вперед або назад у часі.
cmd-replay-skip-help = replay_skip <тик або проміжок часу>
cmd-replay-skip-hint = Тики або часовий проміжок (ЧЧ:ММ:СС).

cmd-replay-set-time-desc = Переходьте вперед або назад до певного часу.
cmd-replay-set-time-help = replay_set <тик або час>
cmd-replay-set-time-hint = Тик або часовий проміжок (HH:MM:SS), починаючи з

cmd-replay-error-time = "{$time}" не є цілим чи проміжком часу.
cmd-replay-error-args = Неправильна кількість аргументів.
cmd-replay-error-no-replay = Повтор не відтворюється.
cmd-replay-error-already-loaded = Повтор вже завантажено.
cmd-replay-error-run-level = Ви не можете завантажити повтор під час з'єднання з сервером.

# Команди запису

cmd-replay-recording-start-desc = Запускає запис повторного відтворення, опціонально з певним обмеженням часу.
cmd-replay-recording-start-help = Використання: replay_recording_start [ім'я] [перезаписати] [обмеження часу]
cmd-replay-recording-start-success = Почав записувати повтор.
cmd-replay-recording-start-already-recording = Вже записую повтор.
cmd-replay-recording-start-error = Виникла помилка при спробі почати запис.
cmd-replay-recording-start-hint-time = [часовий ліміт (хвилини)]
cmd-replay-recording-start-hint-name = [ім'я]
cmd-replay-recording-start-hint-overwrite = [overwrite (bool)]

cmd-replay-recording-stop-desc = Зупиняє відтворення запису.
cmd-replay-recording-stop-help = Використання: replay_recording_stop
cmd-replay-recording-stop-success = Зупинився запис повтору.
cmd-replay-recording-stop-not-recording = Наразі ми не записуємо повтор.

cmd-replay-recording-stats-desc = Відображає інформацію про поточний запис відтворення.
cmd-replay-recording-stats-help = Використання: replay_recording_stats
cmd-replay-recording-stats-result = Тривалість: {$time} хв, Тіки: {$ticks}, Розмір: {$size} МБ, швидкість: {$rate} МБ/хв.


# Інтерфейс керування часом
replay-time-box-scrubbing-label = Динамічне очищення
replay-time-box-replay-time-label = Час запису: {$current} / {$end} ({$percentage}%)
replay-time-box-server-time-label = Час сервера: {$current} / {$end}
replay-time-box-index-label = Індекс: {$current} / {$total}
replay-time-box-tick-label = Тік: {$current} / {$total}
