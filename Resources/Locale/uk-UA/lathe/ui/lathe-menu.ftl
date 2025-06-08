lathe-menu-title = Меню верстата
lathe-menu-queue = Черга
lathe-menu-server-list = Список серверів
lathe-menu-sync = Синхронізація
lathe-menu-search-designs = Пошукові конструкції
lathe-menu-category-all = Усе
lathe-menu-search-filter = Фільтр:
lathe-menu-amount = Сума:
lathe-menu-material-display = {$material} ({$amount})
lathe-menu-tooltip-display = {$amount} штук(-и) {$material}
lathe-menu-description-display = [italic]{$description}[/italic]
lathe-menu-material-amount = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit}
    *[other] {NATURALFIXED($amount, 2)} {$unit}ів(-и)
}
lathe-menu-no-materials-message = Матеріали не завантажені.
lathe-menu-fabricating-message = Виготовлення...
lathe-menu-materials-title = Матеріали
lathe-menu-queue-title = Черга Створення

lathe-menu-material-amount-missing = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit} з {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {$unit} відсутній[/color])
    *[other] {NATURALFIXED($amount, 2)} {$unit}ів з {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {$unit}ів відсутні[/color])
}

lathe-menu-reagent-slot-examine = Збоку є гніздо для мензурки.
lathe-reagent-dispense-no-container = Рідина виливається з {THE($name)} на підлогу!
lathe-menu-result-reagent-display = {$reagent} ({$amount}u)
lathe-menu-connected-to-silo-message = З'єднаний з силосом для матеріалів.