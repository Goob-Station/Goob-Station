### UI

# Shown when a stack is examined in details range
comp-stack-examine-detail-count = {$count ->
    [one] Містить [color={$markupCountColor}]{$count}[/color] штуку
    *[other] Містить [color={$markupCountColor}]{$count}[/color] штук
} в стопці.

# Stack status control
comp-stack-status = Рахунок: [color=white]{$count}[/color]

### Interaction Messages

# Shown when attempting to add to a stack that is full
comp-stack-already-full = Стопка вже повна.

# Shown when a stack becomes full
comp-stack-becomes-full = Тепер стопка повна.

# Text related to splitting a stack
comp-stack-split = Ви поділили стопку
comp-stack-split-halve = Розділити
comp-stack-split-too-small = Стопка замала щоб її поділити

comp-stack-split-custom = Розділити кількість...
comp-stack-split-size = Макс: {$size}
ui-custom-stack-split-title = Розділити кількість
ui-custom-stack-split-line-edit-placeholder = Кількість
ui-custom-stack-split-apply = Розділити