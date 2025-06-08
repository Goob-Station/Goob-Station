generator-clogged = {$generator} раптово вимкнувся!

portable-generator-verb-start = Запустити генератор
portable-generator-verb-start-msg-unreliable = Запустити генератор. На це може знадобитися декілька спроб.
portable-generator-verb-start-msg-reliable = Запустити генератор.
portable-generator-verb-start-msg-unanchored = Спочатку генератор треба прикрутити!
portable-generator-verb-stop = Зупинити генератор
portable-generator-start-fail = Ви потягнули за шнур, але генератор не запустився.
portable-generator-start-success = Ви смикаєте за шнур, і генератор починає дзижчати.

portable-generator-ui-title = Портативний Генератор
portable-generator-ui-status-stopped = Зупинено:
portable-generator-ui-status-starting = Запускається:
portable-generator-ui-status-running = Запущено:
portable-generator-ui-start = Запустити
portable-generator-ui-stop = Зупинити
portable-generator-ui-target-power-label = Цільова Потужність (кВт):
portable-generator-ui-efficiency-label = Ефективність:
portable-generator-ui-fuel-use-label = Використання палива:
portable-generator-ui-fuel-left-label = Залишок палива:
portable-generator-ui-clogged = У паливному баку виявлено чужорідні домішки!
portable-generator-ui-eject = Вийняти
portable-generator-ui-eta = (~{ $minutes } хв)
portable-generator-ui-unanchored = Відкручений
portable-generator-ui-current-output = Поточний вихід: {$voltage}
portable-generator-ui-network-stats = Мережа:
portable-generator-ui-network-stats-value = { POWERWATTS($supply) } / { POWERWATTS($load) }
portable-generator-ui-network-stats-not-connected = Не підключений

power-switchable-generator-examine = Вихід енергії встановлено на {$voltage}.
power-switchable-generator-switched = Вихід змінено на {$voltage}!

power-switchable-voltage = { $voltage ->
    [HV] [color=orange]Висока напруга[/color]
    [MV] [color=yellow]Середня напруга[/color]
    *[LV] [color=green]Низька напруга[/color]
}
power-switchable-switch-voltage = Перемкнути на {$voltage}

fuel-generator-verb-disable-on = Спочатку треба зупитини генератор!
