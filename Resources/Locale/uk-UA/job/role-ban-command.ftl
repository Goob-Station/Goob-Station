### Localization for role ban command

cmd-roleban-desc = Забороняє гравцеві брати роль
cmd-roleban-help = Використання: roleban <ім'я або ідентифікатор користувача> <робота> <причина> [тривалість у хвилинах, не вказувати або 0 для постійного бану].

## Підказки результату виконання
cmd-roleban-hint-1 = <ім'я або ідентифікатор користувача>
cmd-roleban-hint-2 = <робота>
cmd-roleban-hint-3 = <причина>
cmd-roleban-hint-4 = [тривалість у хвилинах, не вказувати або 0 для перманентної заборони]
cmd-roleban-hint-5 = [суворість]

cmd-roleban-hint-duration-1 = Назавжди
cmd-roleban-hint-duration-2 = 1 день
cmd-roleban-hint-duration-3 = 3 дні
cmd-roleban-hint-duration-4 = 1 тиждень
cmd-roleban-hint-duration-5 = 2 тижні
cmd-roleban-hint-duration-6 = 1 місяць


### Localization for role unban command

cmd-roleunban-desc = Відмінити бан ролі гравця
cmd-roleunban-help = Використання: roleunban <role ban id>

## Completion result hints
cmd-roleunban-hint-1 = <role ban id>


### Локалізація для команди list roleban

cmd-rolebanlist-desc = Перелічує бани ролі користувача
cmd-rolebanlist-help = Використання: <ім'я або ідентифікатор користувача> [включити розбанені].

## Підказки результату завершення
cmd-rolebanlist-hint-1 = <ім'я або ідентифікатор користувача>
cmd-rolebanlist-hint-2 = [включити незаборонене]


cmd-roleban-minutes-parse = {$time} не є дійсною кількістю хвилин.\n{$help}
cmd-roleban-severity-parse = ${severity} не є дійсним ступенем тяжкості\n{$help}.
cmd-roleban-arg-count = Неправильна кількість аргументів.
cmd-roleban-job-parse = Робота {$job} не існує
cmd-roleban-name-parse = Не вдалося знайти гравця з таким іменем.
cmd-roleban-existing = {$target} вже має бан ролі {$role}.
cmd-roleban-success = Видано роль бан {$target} ролі {$role} через причину {$reason} {$length}.

cmd-roleban-inf = назавжди
cmd-roleban-until = через {$expires}

# Department bans
cmd-departmentban-desc = Забороняє гравцю брати ролі, що входять до складу відділу
cmd-departmentban-help = Використання: departmentban <ім'я або ID> <відділ> <причина> [тривалість у хвилинах, пропустіть або 0 для постійної заборони]
