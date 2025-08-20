analysis-console-menu-title = аналитическая консоль
analysis-console-server-list-button = Список серверов
analysis-console-scan-button = Сканировать
analysis-console-scan-tooltip-info = Сканируйте артефакты, чтобы получить данные об их структуре.
analysis-console-print-button = Печать
analysis-console-print-tooltip-info = Распечатать актуальную информацию об артефакте.
analysis-console-extract-button = Извлечь О.И.
analysis-console-extract-button-info = Извлечь очки исследований из артефакта, пропорциональные количеству исследованных узлов.
analysis-console-bias-up = Вверх
analysis-console-bias-down = Вниз
analysis-console-bias-button-info-up = Переключает смещение артефакта при перемещении между его узлами. К поверхности - в сторону нулевой глубины.
analysis-console-bias-button-info-down = Переключает смещение артефакта при перемещении между его узлами. В глубину - к поздним и более опасным эффектам.
analysis-console-info-no-scanner = Анализатор не подключён! Пожалуйста, подключите его с помощью мультитула.
analysis-console-info-no-artifact = Артефакт не найден! Поместите артефакт на платформу, затем просканируйте для получения данных.
analysis-console-info-ready = Все системы запущены. Сканирование готово.
analysis-console-info-depth = ГЛУБИНА: {$depth}
analysis-console-info-triggered-true = АКТИВИРОВАН: ДА
analysis-console-info-triggered-false = АКТИВИРОВАН: НЕТ
analysis-console-info-effect = РЕАКЦИЯ: 
analysis-console-info-trigger = СТИМУЛЯТОР:
analysis-console-info-edges = СОЕДИНЕНИЯ: {$edges}
analysis-console-info-value = НЕИЗВЛЕЧЁННЫЕ_О.И.: {$value}
analysis-console-info-scanner = Сканирование...
analysis-console-info-scanner-paused = Приостановлено.
analysis-console-progress-text =
    { $seconds ->
        [one] T-{ $seconds } секунда
        [few] T-{ $seconds } секунды
       *[other] T-{ $seconds } секунд
    }
analysis-console-no-server-connected = Невозможно извлечь О.И. Сервер не подключен.
analysis-console-no-artifact-placed = На сканере нет артефактов.
analysis-console-no-points-to-extract = Отсутствуют очки для извлечения.
analyzer-artifact-component-upgrade-analysis = длительность анализа
analysis-console-print-popup = Консоль печатает отчёт.
analyzer-artifact-extract-popup = Поверхность артефакта мерцает энергией!
analysis-report-title = Отчёт об артефакте: УЗЕЛ { $id }
analysis-console-no-node = Выберите узел
analysis-console-info-id = [font="Monospace" size=11]ID_УЗЛА:[/font]
analysis-console-info-class = [font="Monospace" size=11]Класс:[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{$class}[/font]
analysis-console-info-locked = [font="Monospace" size=11]Статус:[/font]
analysis-console-info-locked-value = [font="Monospace" size=11][color={ $state ->
    [0] red]Заблокирован
    [1] lime]Разблокирован
    *[2] plum]Активен
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Долговечность:[/font]
analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [true] {$info}
    *[false] Разблокируйте узлы для перехода
}[/color][/font]
analysis-console-extract-value = [font="Monospace" size=11][color=orange]Очков {$id} (+{$value})[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange] Нет разблокированных узлов для извлечения очков[/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Всего очков: {$value}[/color][/font]