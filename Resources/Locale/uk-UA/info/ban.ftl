# ban
cmd-ban-desc = Банити когось
cmd-ban-help = Використання: ban <ім'я або ID користувача> <причина> [тривалість у хвилинах, пропустіть або 0 для постійного бану]
cmd-ban-player = Не вдалося знайти гравця з таким ім'ям.
cmd-ban-invalid-minutes = {$minutes} - недійсна кількість хвилин!
cmd-ban-invalid-severity = {$severity} - недійсна серйозність!
cmd-ban-invalid-arguments = Недійсна кількість аргументів
cmd-ban-hint = <ім'я/ID користувача>
cmd-ban-hint-reason = <причина>
cmd-ban-hint-duration = [тривалість]
cmd-ban-hint-severity = [серйозність]

cmd-ban-hint-duration-1 = Назавжди
cmd-ban-hint-duration-2 = 1 день
cmd-ban-hint-duration-3 = 3 дні
cmd-ban-hint-duration-4 = 1 тиждень
cmd-ban-hint-duration-5 = 2 тижні
cmd-ban-hint-duration-6 = 1 місяць

# ban panel
cmd-banpanel-desc = Відкриває панель бану
cmd-banpanel-help = Використання: banpanel [ім'я або guid користувача]
cmd-banpanel-server = Це не можна використовувати з консолі сервера
cmd-banpanel-player-err = Вказаного гравця не знайдено

# listbans
cmd-banlist-desc = Показати активні бани користувача.
cmd-banlist-help = Використання: banlist <ім'я або ID користувача>
cmd-banlist-empty = Активних банів для {$user} не знайдено
cmd-banlistF-hint = <ім'я/ID користувача>

cmd-ban_exemption_update-desc = Встановити виняток з типу бану для гравця.
cmd-ban_exemption_update-help = Використання: ban_exemption_update <гравець> <прапор> [<прапор> [...]]
    Вкажіть кілька прапорів, щоб дати гравцеві кілька винятків.
    Щоб видалити всі винятки, виконайте цю команду та вкажіть "None" як єдиний прапор.

cmd-ban_exemption_update-nargs = Очікувалося щонайменше 2 аргументи
cmd-ban_exemption_update-locate = Не вдалося знайти гравця '{$player}'.
cmd-ban_exemption_update-invalid-flag = Недійсний прапор '{$flag}'.
cmd-ban_exemption_update-success = Оновлено прапори винятків для '{$player}' ({$uid}).
cmd-ban_exemption_update-arg-player = <гравець>
cmd-ban_exemption_update-arg-flag = <прапор>

cmd-ban_exemption_get-desc = Показати винятки з банів для певного гравця.
cmd-ban_exemption_get-help = Використання: ban_exemption_get <гравець>

cmd-ban_exemption_get-nargs = Очікувався рівно 1 аргумент
cmd-ban_exemption_get-none = Користувач не має винятків з банів.
cmd-ban_exemption_get-show = Користувач має такі винятки з банів: {$flags}.
cmd-ban_exemption_get-arg-player = <гравець>

# Ban panel
ban-panel-title = Панель бану
ban-panel-player = Гравець
ban-panel-ip = IP
ban-panel-hwid = HWID
ban-panel-reason = Причина
ban-panel-last-conn = Використовувати IP та HWID з останнього підключення?
ban-panel-submit = Забанити
ban-panel-confirm = Ви впевнені?
ban-panel-tabs-basic = Основна інформація
ban-panel-tabs-reason = Причина
ban-panel-tabs-players = Список гравців
ban-panel-tabs-role = Інформація про бан ролі
ban-panel-no-data = Ви повинні вказати користувача, IP або HWID для бану
ban-panel-invalid-ip = Не вдалося розпізнати IP адресу. Спробуйте ще раз
ban-panel-select = Виберіть тип
ban-panel-server = Бан сервера
ban-panel-role = Бан ролі
ban-panel-minutes = Хвилини
ban-panel-hours = Години
ban-panel-days = Дні
ban-panel-weeks = Тижні
ban-panel-months = Місяці
ban-panel-years = Роки
ban-panel-permanent = Назавжди
ban-panel-ip-hwid-tooltip = Залиште порожнім та поставте галочку нижче, щоб використовувати дані з останнього підключення
ban-panel-severity = Серйозність:
ban-panel-erase = Видалити повідомлення чату та гравця з раунду

# Ban string
server-ban-string = {$admin} створив бан з серйозністю {$severity}, який закінчується {$expires} для [{$name}, {$ip}, {$hwid}], з причиною: {$reason}
server-ban-string-no-pii = {$admin} створив бан з серйозністю {$severity}, який закінчується {$expires} для {$name} з причиною: {$reason}
server-ban-string-never = ніколи

ban-kick-reason = Вас заблоковано
cmd-banlist-hint = <ім'я/ID користувача>