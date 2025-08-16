# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Localization for role ban command

cmd-roleban-desc = Банить гравця від ролі
cmd-roleban-help = Використання: roleban <ім'я або ID користувача> <професія> <причина> [тривалість у хвилинах, залиште пустим або 0 для постійного бану]

## Підказки для результатів автодоповнення
cmd-roleban-hint-1 = <ім'я або ID користувача>
cmd-roleban-hint-2 = <професія>
cmd-roleban-hint-3 = <причина>
cmd-roleban-hint-4 = [тривалість у хвилинах, залиште пустим або 0 для постійного]
cmd-roleban-hint-5 = [суворість]

cmd-roleban-hint-duration-1 = Постійно
cmd-roleban-hint-duration-2 = 1 день
cmd-roleban-hint-duration-3 = 3 дні
cmd-roleban-hint-duration-4 = 1 тиждень
cmd-roleban-hint-duration-5 = 2 тижні
cmd-roleban-hint-duration-6 = 1 місяць


### Локалізація для команди зняття бану з ролі

cmd-roleunban-desc = Знімає бан з ролі гравця
cmd-roleunban-help = Використання: roleunban <ID бану ролі>

## Підказки для результатів автодоповнення
cmd-roleunban-hint-1 = <ID бану ролі>


### Локалізація для команди списку банів ролей

cmd-rolebanlist-desc = Показує список банів ролей користувача
cmd-rolebanlist-help = Використання: <ім'я або ID користувача> [включити зняті бани]

## Підказки для результатів автодоповнення
cmd-rolebanlist-hint-1 = <ім'я або ID користувача>
cmd-rolebanlist-hint-2 = [включити зняті бани]


cmd-roleban-minutes-parse = {$time} не є дійсною кількістю хвилин.\n{$help}
cmd-roleban-severity-parse = {$severity} не є дійсною суворістю\n{$help}.
cmd-roleban-arg-count = Невірна кількість аргументів.
cmd-roleban-job-parse = Професія {$job} не існує.
cmd-roleban-name-parse = Не вдалося знайти гравця з таким ім'ям.
cmd-roleban-existing = {$target} вже має бан для ролі {$role}.
cmd-roleban-success = Забанено гравця {$target} від ролі {$role} з причиною {$reason} {$length}.

cmd-roleban-inf = назавжди
cmd-roleban-until = до {$expires}

# Бани відділів
cmd-departmentban-desc = Банить гравця від ролей, що складають відділ
cmd-departmentban-help = Використання: departmentban <ім'я або ID користувача> <відділ> <причина> [тривалість у хвилинах, залиште пустим або 0 для постійного]

cmd-roleunban-unable-to-parse-id = Не вдалося розпізнати {$id} як ціле число ідентифікатора бану.
                                   {$help}

## Підказки щодо результатів завершення