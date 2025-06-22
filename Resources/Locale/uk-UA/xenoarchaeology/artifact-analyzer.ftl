analysis-console-menu-title = аналiтична консоль
analysis-console-server-list-button = Список Серверів
analysis-console-scan-button = Сканувати
analysis-console-scan-tooltip-info = Скануйте артефакти, щоб отримати інформацію про їхню структуру.
analysis-console-print-button = Друкувати
analysis-console-print-tooltip-info = Надрукувати поточну інформацію про артефакт.
analysis-console-extract-button = Вилучити
analysis-console-extract-button-info = Вилучіть бали з артефакту на основі нещодовно досліджених вузлів.

analysis-console-info-no-scanner = Аналізатор не підключено! Будь ласка, підключіть його за допомогою мультитула.
analysis-console-info-no-artifact = Артефакт відсутній! Помістіть артефакт на платформу, щоб переглянути інформацію про вузол.
analysis-console-info-ready = Системи працюють і готові до сканування.

analysis-console-info-id = [font="Monospace" size=11]ID:[/font]
analysis-console-info-effect = [font="Monospace" size=11]Ефект:[/font]
analysis-console-info-trigger = [font="Monospace" size=11]Тригери:[/font]

analysis-console-info-scanner = Сканування...
analysis-console-info-scanner-paused = Призупинено.
analysis-console-progress-text = {$seconds ->
    [one] T-{$seconds} секунда
    *[other] T-{$seconds} секунд
}

analyzer-artifact-component-upgrade-analysis = тривалість аналізу

analyzer-artifact-extract-popup = Енергія мерехтить на поверхні артефакту!


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
analysis-console-extract-none = [font="Monospace" size=11][color=orange] У розблокованих вузлах не залишилося очок для вилучення [/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Загальне дослідження: {$value}[/color][/font]
