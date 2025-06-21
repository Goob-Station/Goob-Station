analysis-console-menu-title = аналiтична консоль
analysis-console-server-list-button = Список Серверів
analysis-console-scan-button = Сканувати
analysis-console-scan-tooltip-info = Скануйте артефакти, щоб отримати інформацію про їхню структуру.
analysis-console-print-button = Друкувати
analysis-console-print-tooltip-info = Надрукувати поточну інформацію про артефакт.
analysis-console-extract-button = Вилучити
analysis-console-extract-button-info = Вилучіть бали з артефакту на основі нещодовно досліджених вузлів.

analysis-console-info-no-scanner = Аналізатор не підключено! Будь ласка, підключіть його за допомогою мультитула.
analysis-console-info-no-artifact = Артефакт відсутній! Помістіть артефакт на платформу і проскануйте.
analysis-console-info-ready = Системи працюють і готові до сканування.

analysis-console-info-id = ВУЗОЛ_ID: {$id}
analysis-console-info-depth = ГЛИБИНА: {$depth}
analysis-console-info-triggered-true = АКТИВОВАНО: ТАК
analysis-console-info-triggered-false = АКТИВОВАНО: НІ
analysis-console-info-effect = РЕАКЦІЯ: {$effect}
analysis-console-info-trigger = СТИМУЛ: {$trigger}
analysis-console-info-edges = КРАЇ: {$edges}
analysis-console-info-value = ПОТОЧНА_ЦІННІСТЬ: {$value}

analysis-console-info-scanner = Сканування...
analysis-console-info-scanner-paused = Призупинено.
analysis-console-progress-text = {$seconds ->
    [one] T-{$seconds} секунда
    *[other] T-{$seconds} секунд
}

analyzer-artifact-component-upgrade-analysis = тривалість аналізу

analysis-console-print-popup = Консоль надрукувала звіт.
analyzer-artifact-extract-popup = Енергія мерехтить на поверхні артефакту!

analysis-report-title = Звіт про артефакт: Вузол {$id}

analysis-console-bias-up = Вгору
analysis-console-bias-down = Вниз
analysis-console-bias-button-info-up = Вмикає або вимикає зміщення артефакту при переміщенні між вузлами. Вгору - до нульової глибини.
analysis-console-bias-button-info-down = Перемикає зсув артефакту при переміщенні між вузлами. Вниз - рухається до все більшої глибини.
analysis-console-no-server-connected = Неможливо витягти. Сервер не підключено.
analysis-console-no-artifact-placed = Артефактів на сканері немає.
analysis-console-no-points-to-extract = Ніяких балів, які можна витягти.
analysis-console-no-node = Виберіть вузол для перегляду
analysis-console-info-id-value = [font="Monospace" size=11][color=yellow]{$id}[/color][/font]
analysis-console-info-class = [font="Monospace" size=11]Клас:[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{$class}[/font]
analysis-console-info-locked = [font="Monospace" size=11]Статус:[/font]
analysis-console-info-locked-value = [font="Monospace" size=11][color={ $state ->
    [0] red]Заблоковано
    [1] lime]Розблоковано
    *[2] plum]Активно
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Міцність:[/font]
analysis-console-info-durability-value = [font="Monospace" size=11][color={$color}]{$current}/{$max}[/color][/font]
analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [true] {$info}
    *[false] Розблокуйте вузли, щоб отримати інформацію
}[/color][/font]
analysis-console-info-triggered-value = [font="Monospace" size=11][color=gray]{$triggers}[/color][/font]
analysis-console-extract-value = [font="Monospace" size=11][color=orange]Вузол {$id} (+{$value})[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange] В розблокованих вузлах не залишилося очок для вилучення [/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Загальне дослідження: {$value}[/color][/font]