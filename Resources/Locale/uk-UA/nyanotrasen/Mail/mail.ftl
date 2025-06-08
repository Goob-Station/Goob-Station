# SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

mail-recipient-mismatch = Ім'я або посада одержувача не збігаються.
mail-invalid-access = Ім'я та посада одержувача збігаються, але доступ не відповідає очікуваному.
mail-locked = Замок від несанкціонованого доступу не знято. Торкніться ID одержувача.
mail-desc-far = Посилка. З такої відстані неможливо розібрати, кому вона адресована.
mail-desc-close = Посилка, адресована {CAPITALIZE($name)}, {$job}.
mail-desc-fragile = Має [color=red]червону позначку "крихке"[/color].
mail-desc-priority = [color=yellow]Жовта пріоритетна стрічка[/color] на замку від несанкціонованого доступу активна. Краще доставте її вчасно!
mail-desc-priority-inactive = [color=#886600]Жовта пріоритетна стрічка[/color] на замку від несанкціонованого доступу неактивна.
mail-unlocked = Систему захисту від несанкціонованого доступу розблоковано.
mail-unlocked-by-emag = Система захисту від несанкціонованого доступу *БЗЗЗТ*.
mail-unlocked-reward = Систему захисту від несанкціонованого доступу розблоковано. {$bounty} спесо додано на рахунок логістики.
mail-penalty-lock = ЗАМОК ЗАХИСТУ ВІД ВСКРИТТЯ ЗЛАМАНО. БАНКІВСЬКИЙ РАХУНОК ЛОГІСТИКИ ОШТРАФОВАНО НА {$credits} СПЕСО.
mail-penalty-fragile = ЦІЛІСНІСТЬ ПОРУШЕНО. БАНКІВСЬКИЙ РАХУНОК ЛОГІСТИКИ ОШТРАФОВАНО НА {$credits} СПЕСО.
mail-penalty-expired = ДОСТАВКУ ПРОСТРОЧЕНО. БАНКІВСЬКИЙ РАХУНОК ЛОГІСТИКИ ОШТРАФОВАНО НА {$credits} СПЕСО.
mail-item-name-unaddressed = пошта
mail-item-name-addressed = пошта ({$recipient})

command-mailto-description = Поставити посилку в чергу на доставку до сутності. Приклад використання: `mailto 1234 5678 false false`. Вміст цільового контейнера буде перенесено у справжню поштову посилку.
### Frontier: додати опис is-large
command-mailto-help = Використання: {$command} <recipient entityUid> <container entityUid> [is-fragile: true або false] [is-priority: true або false] [is-large: true або false, опціонально]
command-mailto-no-mailreceiver = Цільова сутність-одержувач не має {$requiredComponent}.
command-mailto-no-blankmail = Прототип {$blankMail} не існує. Щось пішло не так. Зв'яжіться з програмістом.
command-mailto-bogus-mail = {$blankMail} не мав {$requiredMailComponent}. Щось пішло не так. Зв'яжіться з програмістом.
command-mailto-invalid-container = Цільова сутність-контейнер не має контейнера {$requiredContainer}.
command-mailto-unable-to-receive = Не вдалося налаштувати цільову сутність-одержувача для отримання пошти. Можливо, відсутній ID.
command-mailto-no-teleporter-found = Не вдалося знайти відповідний поштовий телепортер станції для цільової сутності-одержувача. Можливо, одержувач знаходиться поза станцією.
command-mailto-success = Успіх! Поштова посилка поставлена в чергу на наступну телепортацію через {$timeToTeleport} секунд.

command-mailnow = Примусово змусити всі поштові телепортери доставити наступну партію пошти якомога швидше. Це не обійде ліміт недоставленої пошти.
command-mailnow-help = Використання: {$command}
command-mailnow-success = Успіх! Усі поштові телепортери незабаром доставлять наступну партію пошти.
