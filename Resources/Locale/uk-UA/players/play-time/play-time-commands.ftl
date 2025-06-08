parse-minutes-fail = Не вдається розібрати '{$minutes}' як хвилини
parse-session-fail = Не знайдено сесію для '{$username}'

## Команди рольового таймера

# - playtime_addoverall
cmd-playtime_addoverall-desc = Додає вказані хвилини до загального ігрового часу гравця
cmd-playtime_addoverall-help = Usage: {$command} <user name> <minutes>
cmd-playtime_addoverall-succeed = Збільшено загальний час для {$username} до {TOSTRING($time, "dddd\\:hh\\:mm")}
cmd-playtime_addoverall-arg-user = <ім'я користувача
cmd-playtime_addoverall-arg-minutes = <хвилини>
cmd-playtime_addoverall-error-args = Очікувані рівно два аргументи

# - playtime_addrole
cmd-playtime_addrole-desc = Додає вказані хвилини до часу рольової гри гравця
cmd-playtime_addrole-help = Використання: {$command} <ім'я користувача> <роль> <хвилини>
cmd-playtime_addrole-succeed = Збільшено час гри для {$username} / \'{$role}\' до {TOSTRING($time, "dddd\\:hh\\:mm")}
cmd-playtime_addrole-arg-user = <ім'я користувача
cmd-playtime_addrole-arg-role = <role>
cmd-playtime_addrole-arg-minutes = <хвилини>
cmd-playtime_addrole-error-args = Очікувані рівно три аргументи

# - playtime_getoverall
cmd-playtime_getoverall-desc = Отримує вказані хвилини для загального ігрового часу гравця
cmd-playtime_getoverall-help = Usage: {$command} <user name>
cmd-playtime_getoverall-success = Загальний час для {$username} дорівнює {TOSTRING($time, "dddd\\:hh\\:mm")}.
cmd-playtime_getoverall-arg-user = <ім'я користувача
cmd-playtime_getoverall-error-args = Очікувався рівно один аргумент

# - GetRoleTimer
cmd-playtime_getrole-desc = Отримує всі або один таймер ролі від гравця
cmd-playtime_getrole-help = Usage: {$command} <user name> [role]
cmd-playtime_getrole-no = Не знайдено таймерів ролей
cmd-playtime_getrole-role = Роль: {$role}, Час гри: {$time}
cmd-playtime_getrole-overall = Загальний час гри становить {$time}
cmd-playtime_getrole-succeed = Час гри для {$username} є: {TOSTRING($time, "dddd\\:hh\\:mm")}.
cmd-playtime_getrole-arg-user = <ім'я користувача
cmd-playtime_getrole-arg-role = <role|'Overall'>
cmd-playtime_getrole-error-args = Очікується рівно один-два аргументи

# - playtime_save
cmd-playtime_save-desc = Зберігає час гри гравця в БД
cmd-playtime_save-help = Usage: {$command} <user name>
cmd-playtime_save-succeed = Збережено ігровий час для {$username}
cmd-playtime_save-arg-user = <ім'я користувача
cmd-playtime_save-error-args = Очікувався рівно один аргумент

## Команда 'playtime_flush''

cmd-playtime_flush-desc = Змити активні трекери до збережених у відстеженні ігрового часу.
cmd-playtime_flush-help = Usage: {$command} [user name]
    This causes a flush to the internal storage only, it does not flush to DB immediately.
    If a user is provided, only that user is flushed.

cmd-playtime_flush-error-args = Очікувані нуль або один аргумент
cmd-playtime_flush-arg-user = [ім'я користувача]

cmd-playtime_unlock-desc = Розблокуйте вимогу до ігрового часу для певних завдань.
cmd-playtime_unlock-help = Використання: {$command} [ім'я користувача] [трекери...]
    Ця команда знімає вимоги до ігрового часу для певних професій для користувача.
cmd-playtime_unlock-arg-user = [ім'я користувача]
cmd-playtime_unlock-arg-job = [ідентифікатор роботи]
cmd-playtime_unlock-error-args = Очікувані нуль або один аргумент
cmd-playtime_unlock-error-job = Очікували валідний JobPrototype для другого аргументу, а отримали {$invalidJob}.
cmd-playtime_unlock-error-no-requirements = Вимог до CharacterPlaytimeRequirements або CharacterDepartmentTime не знайдено.