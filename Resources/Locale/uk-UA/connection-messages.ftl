whitelist-not-whitelisted = Ви не у вайтлісті.

# proper handling for having a min/max or not
whitelist-playercount-invalid = {$min ->
    [0] Білий список для цього сервера застосовується лише до гравців нижче {$max}.
    *[other] Білий список для цього сервера застосовується лише до гравців вище {$min}. {$max ->
        [2147483647] -> гравців, тому ви зможете приєднатися пізніше.
       *[other] -> гравці та нижче {$max} гравців, тому ви зможете приєднатися пізніше.
    }
}
whitelist-not-whitelisted-rp = Ви не у вайтлісті. Якщо ви досвідчений гравець, щоб вас додали у вайтліст, зайдіть у наш Дискорд (посилання в лаунчері) та створіть тікет.

cmd-whitelistadd-desc = Додає гравця з зазначеним ніком до вайтліста.
cmd-whitelistadd-help = whitelistadd <нік>
cmd-whitelistadd-existing = {$username} вже у вайтлісті!
cmd-whitelistadd-added = {$username} додано у вайтліст
cmd-whitelistadd-not-found = Не вийшло знайти '{$username}'
cmd-whitelistadd-arg-player = [гравець]

cmd-whitelistremove-description = Видалити гравця з таким ніком з вайтлісту.
cmd-whitelistremove-help = whitelistremove <нік>
cmd-whitelistremove-existing = {$username} не у вайтлісті!
cmd-whitelistremove-removed = {$username} видалено з вайтліста
cmd-whitelistremove-not-found = Неможливо знайти '{$username}'
cmd-whitelistremove-arg-player = [гравець]

cmd-kicknonwhitelisted-description = Кікнути всіх гравців не у вайтлісті з сервера.
cmd-kicknonwhitelisted-help = Використання: kicknonwhitelisted

ban-banned-permanent = Цього бану можна позбавитись лише оскарежнням.
ban-banned-permanent-appeal = Цього бану можна позбутись лише поданням апеляції. Ви можете подати апеляцію в {$link}
ban-expires = Цей бан триватиме {$duration} хвилин і він скінчиться в {$time} UTC (час Лондона).
ban-banned-1 = Ви або інший користувач цього пристрою або мережі забанені на цьому сервері.
ban-banned-2 = Адміністратор: "{$adminName}"
ban-banned-2 = Причина бану: "{$reason}"
ban-banned-3 = Спроба обійти бан, наприклад створенням нового профіля, буде записана

soft-player-cap-full = Сервер повний!
panic-bunker-account-denied = Сервер у режимі панічного бункера. Нові підключення не будуть прийняті. Спробуйте пізніше
panic-bunker-account-denied-reason = Сервер у режимі панічного бункера, вас не підключило. Причина: "{$reason}"
panic-bunker-account-reason-account = Акаунт SS14 має бути старшим за {$minutes} хвилин
panic-bunker-account-reason-overall = Кількість награних годин має бути {$hours} годин

cmd-whitelistremove-desc = Видаляє гравця з вказаним іменем користувача з білого списку сервера.
cmd-kicknonwhitelisted-desc = Виганяє з сервера всіх гравців, які не входять до білого списку.
whitelist-playtime = У вас недостатньо ігрового часу, щоб приєднатися до цього серверу. Вам потрібно щонайменше {$hours} хвилин ігрового часу, щоб приєднатися до цього серверу.
whitelist-player-count = Цей сервер зараз не приймає гравців. Будь ласка, спробуйте пізніше.
whitelist-notes = Наразі ви маєте занадто багато приміток адміністратора, щоб приєднатися до цього сервера. Ви можете перевірити свої нотатки, набравши /adminremarks у чаті.
whitelist-manual = Вас не внесено до білого списку на цьому сервері.
whitelist-blacklisted = Вас внесено до чорного списку цього сервера.
whitelist-always-deny = Вам не дозволено приєднатися до цього сервера.
whitelist-fail-prefix = Не внесено до білого списку: {$msg}
whitelist-misconfigured = Сервер неправильно налаштований і не приймає гравців. Будь ласка, зв'яжіться з власником сервера і повторіть спробу пізніше.
cmd-blacklistadd-desc = Додає гравця з вказаним іменем користувача до чорного списку сервера.
cmd-blacklistadd-help = Використання: blacklistadd <ім'я користувача>
cmd-blacklistadd-existing = {$username} вже в чорному списку!
cmd-blacklistadd-added = {$username} додано до чорного списку
cmd-blacklistadd-not-found = Не вдалося знайти '{$username}'
cmd-blacklistadd-arg-player = [гравець]
cmd-blacklistremove-desc = Видаляє гравця з заданим іменем користувача з чорного списку сервера.
cmd-blacklistremove-help = Використання: blacklistremove <ім'я користувача
cmd-blacklistremove-existing = {$username} is not on the blacklist!
cmd-blacklistremove-removed = {$username} removed from the blacklist
cmd-blacklistremove-not-found = Не вдалося знайти '{$username}'
cmd-blacklistremove-arg-player = [гравець]
baby-jail-account-denied = Це сервер для новачків, призначений для нових гравців і тих, хто хоче їм допомогти. Нові підключення від акаунтів, які є надто старими або не входять до білого списку, не приймаються. Перевірте інші сервери та побачте все, що може запропонувати Space Station 14. Приємної гри!
baby-jail-account-denied-reason = Це сервер для новачків, призначений для нових гравців та тих, хто хоче їм допомогти.Нові підключення від занадто старих акаунтів або акаунтів, які не входять до білого списку, не приймаються.Перевірте інші сервери та побачите все, що може запропонувати Space Station 14.Приємної вам гри!Причина: "{$reason}"
baby-jail-account-reason-account = Ваш акаунт Space Station 14 застарілий. Він має бути молодшим, ніж {$hours} годин
baby-jail-account-reason-overall = Ваш загальний час гри на сервері не повинен перевищувати {$hours} годин
ban-banned-4 = Спроби обійти цей бан, наприклад, шляхом створення нового облікового запису, будуть зареєстровані.
generic-misconfigured = Сервер неправильно налаштований і не приймає гравців. Будь ласка, зв'яжіться з власником сервера і спробуйте пізніше.
ipintel-server-ratelimited = Цей сервер використовує систему безпеки із зовнішньою верифікацією, яка досягла максимального ліміту перевірок. Будь ласка, зв'яжіться з командою адміністрації сервера для отримання допомоги та спробуйте пізніше.
ipintel-unknown = Цей сервер використовує систему безпеки із зовнішньою верифікацією, але виникла помилка. Будь ласка, зв'яжіться з командою адміністрації сервера для отримання допомоги та спробуйте пізніше.
ipintel-suspicious = Схоже, ви підключаєтеся через дата-центр або VPN. З адміністративних причин ми не дозволяємо використовувати VPN-з'єднання для гри. Будь ласка, зв'яжіться з командою адміністрації сервера, якщо вважаєте, що це помилка.
hwid-required = Ваш клієнт відмовився надіслати ідентифікатор обладнання. Будь ласка, зв'яжіться з командою адміністрації для подальшої допомоги.
