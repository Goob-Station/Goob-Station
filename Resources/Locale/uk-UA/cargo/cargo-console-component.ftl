## UI
cargo-console-menu-title = Консоль подачі заявок на вантаж
cargo-console-menu-account-name-label = Назва рахунку:{" "}
cargo-console-menu-account-name-none-text = Немає
cargo-console-menu-shuttle-name-label = Ім'я шатла:{" "}
cargo-console-menu-shuttle-name-none-text = Немає
cargo-console-menu-points-label = Космобаксів:{" "}
cargo-console-menu-points-amount = ${$amount}
cargo-console-menu-shuttle-status-label = Статус шатлу:{" "}
cargo-console-menu-shuttle-status-away-text = Відлетів
cargo-console-menu-order-capacity-label = Обсяг замовлення:{" "}
cargo-console-menu-call-shuttle-button = Активувати телепад
cargo-console-menu-permissions-button = Дозволи
cargo-console-menu-categories-label = Категорії:{" "}
cargo-console-menu-search-bar-placeholder = Шукати
cargo-console-menu-requests-label = Заявки
cargo-console-menu-orders-label = Замовлення
cargo-console-menu-order-reason-description = Причини: {$reason}
cargo-console-menu-populate-categories-all-text = Усі
cargo-console-menu-populate-orders-cargo-order-row-product-name-text = {$productName} (x{$orderAmount}) від {$orderRequester}
cargo-console-menu-cargo-order-row-approve-button = Затвердити
cargo-console-menu-cargo-order-row-cancel-button = Відмовити

# Orders
cargo-console-order-not-allowed = Доступ заборонено
cargo-console-station-not-found = Немає доступної станції
cargo-console-invalid-product = Невірний ідентифікатор товару
cargo-console-too-many = Занадто багато затверджених наказів
cargo-console-snip-snip = Замовлення урізано до мінімуму
cargo-console-insufficient-funds = Недостатньо коштів (потрібно: {$cost})
cargo-console-unfulfilled = Не вистачає місця для виконання замовлення
cargo-console-trade-station = Відправлено до {$destination}

cargo-console-paper-print-name = Замовлення #{$orderNumber}
cargo-console-paper-print-text = [head=2]Замовлення #{$orderNumber}[/head]
    {"[bold]Предмет:[/bold]"} {$itemName} (x{$orderQuantity})
    {"[bold]Замовлено:[/bold]"} {$requester}

    {"[head=3]Інформація про замовлення[/head]"}
    {"[bold]Платник[/bold]:"} {$account} [font="Monospace"]\[{$accountcode}\][/font]
    {"[bold]Затверджено:[/bold]"} {$approver}
    {"[bold]Причина:[/bold]"} {$reason}

# Cargo shuttle console
cargo-shuttle-console-menu-title = Консоль вантажного шаттла
cargo-shuttle-console-station-unknown = Невідомо
cargo-shuttle-console-shuttle-not-found = Не знайдено
cargo-shuttle-console-organics = Виявлено органічні форми життя на шатлі
cargo-no-shuttle = Вантажний шатл не знайдено!

cargo-telepad-delay-upgrade = Затримка телепортації

cargo-console-unlock-approved-order-broadcast = [bold]{$productName} x{$orderAmount}[/bold], який коштував [bold]{$cost}[/bold], був затверджений [bold]{$approver}[/bold]

cargo-console-cooldown-count = Неможливо замовити більше одного {$product} за раз.
cargo-console-cooldown-active = Замовлення на {$product} неможливо розмістити ще протягом {$timeCount} {$timeUnits}.
cargo-console-menu-account-name-format = [bold][color={$color}]{$name}[/color][/bold] [font="Monospace"]\[{$code}\][/font]
cargo-console-menu-tab-title-orders = Замовлення
cargo-console-menu-tab-title-funds = Перекази
cargo-console-menu-account-action-transfer-limit = [bold]Ліміт переказу:[/bold] ${$limit}
cargo-console-menu-account-action-transfer-limit-unlimited-notifier = [color=gold](Необмежено)[/color]
cargo-console-menu-account-action-select = [bold]Дія з рахунком:[/bold]
cargo-console-menu-account-action-amount = [bold]Сума:[/bold] $
cargo-console-menu-account-action-button = Переказати
cargo-console-menu-toggle-account-lock-button = Перемкнути ліміт переказу
cargo-console-menu-account-action-option-withdraw = Зняти готівку
cargo-console-menu-account-action-option-transfer = Переказати кошти до {$code}

# Замовлення
cargo-console-fund-withdraw-broadcast = [bold]{$name} зняв(ла) {$amount} спесо з {$name1} \[{$code1}\]
cargo-console-fund-transfer-broadcast = [bold]{$name} переказав(ла) {$amount} спесо з {$name1} \[{$code1}\] до {$name2} \[{$code2}\][/bold]
cargo-console-fund-transfer-user-unknown = Невідомо

# GoobStation - час відновлення на вантажні замовлення (особливо азартні ігри)
cargo-console-paper-reason-default = Відсутня
cargo-console-paper-approver-default = Власноруч
cargo-funding-alloc-console-menu-title = Консоль розподілу фінансування
cargo-funding-alloc-console-label-account = [bold]Рахунок[/bold]
cargo-funding-alloc-console-label-code = [bold] Код [/bold]
cargo-funding-alloc-console-label-balance = [bold] Баланс [/bold]
cargo-funding-alloc-console-label-cut = [bold] Розподіл доходу (%) [/bold]
cargo-funding-alloc-console-label-primary-cut = Частка Карго від коштів з джерел, що не є сейфами (%):
cargo-funding-alloc-console-label-lockbox-cut = Частка Карго від продажу сейфів (%):
cargo-funding-alloc-console-label-help-non-adjustible = Карго отримує {$percent}% прибутку від продажів, що не стосуються сейфів. Решта розподіляється, як зазначено нижче:
cargo-funding-alloc-console-label-help-adjustible = Решта коштів з джерел, що не є сейфами, розподіляється, як зазначено нижче:
cargo-funding-alloc-console-button-save = Зберегти зміни
cargo-funding-alloc-console-label-save-fail = [bold]Невірний розподіл доходу![/bold] [color=red]({$pos ->
    [1] +
    *[-1] -
}{$val}%)[/color]
