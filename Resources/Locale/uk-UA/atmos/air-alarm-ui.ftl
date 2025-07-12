# UI

## Window

air-alarm-ui-access-denied = Недостатній доступ!

air-alarm-ui-window-pressure-label = Тиск
air-alarm-ui-window-temperature-label = Температура
air-alarm-ui-window-alarm-state-label = Стан

air-alarm-ui-window-address-label = Адреса
air-alarm-ui-window-device-count-label = Всього Пристроїв
air-alarm-ui-window-resync-devices-label = Пересинхронізувати

air-alarm-ui-window-mode-label = Режим
air-alarm-ui-window-auto-mode-label = Автоматичний режим

air-alarm-ui-window-pressure = {$pressure} кПа
air-alarm-ui-window-pressure-indicator = Тиск: [color={$color}]{$pressure} кПа[/color]
air-alarm-ui-window-temperature = {$tempC} Ц ({$temperature} K)
air-alarm-ui-window-temperature-indicator = Температура: [color={$color}]{$tempC} Ц ({$temperature} K)[/color]
air-alarm-ui-window-alarm-state = [color={$color}]{-air-alarm-state-name(state:$state)}[/color]
air-alarm-ui-window-alarm-state-indicator = Стан: [color={$color}]{$state}[/color]

air-alarm-ui-window-tab-vents = Вентиляція
air-alarm-ui-window-tab-scrubbers = Скруббери
air-alarm-ui-window-tab-sensors = Сенсори

air-alarm-ui-gases = {$gas}: {$amount} моль ({$percentage}%)
air-alarm-ui-gases-indicator = {$gas}: [color={$color}]{$amount} моль ({$percentage}%)[/color]

air-alarm-ui-mode-filtering = Фільтрація
air-alarm-ui-mode-wide-filtering = Фільтрування (широке)
air-alarm-ui-mode-fill = Заповнення
air-alarm-ui-mode-panic = Всасування (Паніка)
air-alarm-ui-mode-none = Немає

## Widgets

### General

air-alarm-ui-widget-enable = Ввімкнено
air-alarm-ui-widget-copy = Копіювати налаштування на подібні пристрої
air-alarm-ui-widget-copy-tooltip = Копіює налаштування цього пристрою на всі пристрої на цій вкладці повітряної тривоги.
air-alarm-ui-widget-ignore = Ігнорувати
air-alarm-ui-atmos-net-device-label = Адреса: {$address}

### Vent pumps

air-alarm-ui-vent-pump-label = Напрямок вентиляції
air-alarm-ui-vent-pressure-label = Обмеження тиску
air-alarm-ui-vent-external-bound-label = Зовнішнє обмеження
air-alarm-ui-vent-internal-bound-label = Внутрішнє обмеження

### Scrubbers

air-alarm-ui-scrubber-pump-direction-label = Напрямок
air-alarm-ui-scrubber-volume-rate-label = Швидкість (Л)
air-alarm-ui-scrubber-wide-net-label = Широка мережа (WideNet)

### Thresholds

air-alarm-ui-sensor-gases = Гази
air-alarm-ui-sensor-thresholds = Пороги
air-alarm-ui-thresholds-pressure-title = Порогові значення (кПа)
air-alarm-ui-thresholds-temperature-title = Порогові значення (К)
air-alarm-ui-thresholds-gas-title = Порогові значення (%)
air-alarm-ui-thresholds-upper-bound = Небезпека вище
air-alarm-ui-thresholds-lower-bound = Небезпека нижче
air-alarm-ui-thresholds-upper-warning-bound = Попередження вище
air-alarm-ui-thresholds-lower-warning-bound = Попередження нижче

-air-alarm-state-name = { $state ->
    [normal] Норма
    [warning] Попередження
    [danger] Небезпека
    [emagged] Зламано
   *[invalid] Невірно
}
air-alarm-ui-window-listing-title = {$address} : {-air-alarm-state-name(state:$state)}
air-alarm-ui-thresholds-copy = Копіювати пороги на всі пристрої
air-alarm-ui-thresholds-copy-tooltip = Копіює порогові значення датчиків цього пристрою на всі пристрої в цій вкладці повітряної тривоги.
air-alarm-ui-window-mode-select-locked-label = [bold][color=red] Помилка селектора режимів! [/color][/bold]