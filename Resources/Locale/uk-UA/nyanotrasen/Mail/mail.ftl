# SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

mail-recipient-mismatch = Ім'я або посада одержувача не збігаються.
mail-invalid-access = Ім'я та посада одержувача збігаються, але доступ не відповідає очікуваному.
mail-locked = Замок захисту від несанкціонованого доступу не знято. Торкніться ID-картки одержувача.
mail-desc-far = Посилка. З такої відстані неможливо розібрати, кому вона адресована.
mail-desc-close = Посилка, адресована {CAPITALIZE($name)}, {$job}.
mail-desc-fragile = На ній є [color=red]червона наліпка "крихке"[/color].
mail-desc-priority = [color=yellow]Жовта пріоритетна стрічка[/color] замка захисту від втручання активна. Краще доставити її вчасно!
mail-desc-priority-inactive = [color=#886600]Жовта пріоритетна стрічка[/color] замка захисту від втручання неактивна.
mail-unlocked = Система захисту від втручання розблокована.
mail-unlocked-by-emag = Система захисту від втручання *БЗЗЗТ*.
mail-unlocked-reward = Система захисту від втручання розблокована. {$bounty} спесо додано на рахунок відділу логістики.
mail-penalty-lock = ЗАМОК ЗАХИСТУ ВІД ВТРУЧАННЯ ЗЛАМАНО. БАНКІВСЬКИЙ РАХУНОК ЛОГІСТИКИ ОШТРАФОВАНО НА {$credits} СПЕСО.
mail-penalty-fragile = ЦІЛІСНІСТЬ ПОРУШЕНО. БАНКІВСЬКИЙ РАХУНОК ЛОГІСТИКИ ОШТРАФОВАНО НА {$credits} СПЕСО.
mail-penalty-expired = ТЕРМІН ДОСТАВКИ ПРОСТРОЧЕНО. БАНКІВСЬКИЙ РАХУНОК ЛОГІСТИКИ ОШТРАФОВАНО НА {$credits} СПЕСО.
mail-item-name-unaddressed = пошта
mail-item-name-addressed = пошта ({$recipient})


command-mailto-description = Поставити посилку в чергу на доставку до об'єкта. Приклад використання: `mailto 1234 5678 false false`. Вміст цільового контейнера буде переміщено до справжньої поштової посилки.
### Frontier: додати опис is-large
command-mailto-help = Використання: {$command} <ID_сутності_отримувача> <ID_сутності_контейнера> [крихке: true або false] [пріоритетне: true або false] [велике: true або false, необов'язково]
command-mailto-no-mailreceiver = Цільовий об'єкт-одержувач не має {$requiredComponent}.
command-mailto-no-blankmail = Прототип {$blankMail} не існує. Щось дуже негаразд. Зв'яжіться з програмістом.
command-mailto-bogus-mail = {$blankMail} не мав {$requiredMailComponent}. Щось дуже негаразд. Зв'яжіться з програмістом.
command-mailto-invalid-container = Цільовий об'єкт-контейнер не має контейнера {$requiredContainer}.
command-mailto-unable-to-receive = Не вдалося налаштувати цільовий об'єкт-одержувач для отримання пошти. Можливо, відсутній ID.
command-mailto-no-teleporter-found = Не вдалося знайти відповідний поштовий телепорт на станції для цільового об'єкта-одержувача. Одержувач може бути поза станцією.
command-mailto-success = Успіх! Поштова посилка додана в чергу на наступну телепортацію через {$timeToTeleport} секунд.
command-mailnow = Примусово змусити всі поштові телепорти доставити наступну партію пошти якомога швидше. Це не обійде ліміт недоставленої пошти.
command-mailnow-help = Використання: {$command}
command-mailnow-success = Успіх! Усі поштові телепорти незабаром доставлять наступну партію пошти.