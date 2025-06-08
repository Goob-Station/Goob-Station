device-pda-slot-component-slot-name-cartridge = Картридж

default-program-name = Програма
notekeeper-program-name = Замітки
news-read-program-name = Новини станції

crew-manifest-program-name = Маніфест екіпажу
crew-manifest-cartridge-loading = Загрузка ...

net-probe-program-name = НетПроб
net-probe-scan = Скановано {$device}!
net-probe-label-name = Імʼя
net-probe-label-address = Адреса
net-probe-label-frequency = Частота
net-probe-label-network = Мережа

log-probe-program-name = ЛогПроб
log-probe-scan = Викачано логи з {$device}!
log-probe-label-time = Час
log-probe-label-accessor = Доступ до
log-probe-label-number = #

glimmer-monitor-program-name = Монітор Мерехтіння
glimmer-monitor-current-glimmer = Мерехтіння: {$glimmer}Ψ
glimmer-monitor-interval = Інтервал
glimmer-monitor-sync = Синхронізувати

astro-nav-program-name = АстроНав
med-tek-program-name = МедТек
nano-task-program-name = НаноЗавдання
log-probe-print-button = Роздрукувати логи
log-probe-printout-device = Просканований пристрій: {$name}
log-probe-printout-header = Останні логи:
log-probe-printout-entry = #{$number} / {$time} / {$accessor}
nano-task-ui-heading-high-priority-tasks = { $amount ->
        [zero] Немає завдань з високим пріоритетом
        [one] 1 завдання з високим пріоритетом
       *[other] {$amount} завдань з високим пріоритетом
    }
nano-task-ui-heading-medium-priority-tasks = { $amount ->
        [zero] Немає завдань із середнім пріоритетом
        [one] 1 завдання із середнім пріоритетом
       *[other] {$amount} завдань із середнім пріоритетом
    }
nano-task-ui-heading-low-priority-tasks = { $amount ->
        [zero] Немає завдань з низьким пріоритетом
        [one] 1 завдання з низьким пріоритетом
       *[other] {$amount} завдань з низьким пріоритетом
    }
nano-task-ui-done = Готово
nano-task-ui-revert-done = Скасувати
nano-task-ui-priority-low = Низький
nano-task-ui-priority-medium = Середній
nano-task-ui-priority-high = Високий
nano-task-ui-cancel = Скасувати
nano-task-ui-print = Друк
nano-task-ui-delete = Видалити
nano-task-ui-save = Зберегти
nano-task-ui-new-task = Нове завдання
nano-task-ui-description-label = Опис:
nano-task-ui-description-placeholder = Зробити щось важливе
nano-task-ui-requester-label = Замовник:
nano-task-ui-requester-placeholder = Іван Нанотрейзен
nano-task-ui-item-title = Редагувати завдання
nano-task-printed-description = Опис: {$description}
nano-task-printed-requester = Замовник: {$requester}
nano-task-printed-high-priority = Пріоритет: Високий
nano-task-printed-medium-priority = Пріоритет: Середній
nano-task-printed-low-priority = Пріоритет: Низький

# Картридж зі списком розшукуваних
wanted-list-program-name = Список розшукуваних
wanted-list-label-no-records = Все гаразд, ковбою
wanted-list-search-placeholder = Пошук за іменем та статусом
wanted-list-age-label = [color=darkgray]Вік:[/color] [color=white]{$age}[/color]
wanted-list-job-label = [color=darkgray]Робота:[/color] [color=white]{$job}[/color]
wanted-list-species-label = [color=darkgray]Вид:[/color] [color=white]{$species}[/color]
wanted-list-gender-label = [color=darkgray]Стать:[/color] [color=white]{$gender}[/color]
wanted-list-reason-label = [color=darkgray]Причина:[/color] [color=white]{$reason}[/color]
wanted-list-unknown-reason-label = причина невідома
wanted-list-initiator-label = [color=darkgray]Ініціатор:[/color] [color=white]{$initiator}[/color]
wanted-list-unknown-initiator-label = ініціатор невідомий
wanted-list-status-label = [color=darkgray]статус:[/color] {$status ->
        [suspected] [color=yellow]підозрюваний[/color]
        [wanted] [color=red]розшукується[/color]
        [detained] [color=#b18644]затриманий[/color]
        [paroled] [color=green]умовно-достроково звільнений[/color]
        [discharged] [color=green]звільнений[/color]
        *[other] немає
    }
wanted-list-history-table-time-col = Час
wanted-list-history-table-reason-col = Злочин
wanted-list-history-table-initiator-col = Ініціатор