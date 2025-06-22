# Mailto
command-mailto-description = Поставити посилку в чергу на доставку до сутності. Приклад використання: `mailto 1234 5678 false false`. Вміст цільового контейнера буде переміщено до фактичної поштової посилки.
### Frontier: додати опис is-large
command-mailto-help = Використання: {$command} <entityUid одержувача> <entityUid контейнера> [is-fragile: true або false] [is-priority: true або false] [is-large: true або false, опціонально]
command-mailto-no-mailreceiver = Цільова сутність-одержувач не має {$requiredComponent}.
command-mailto-no-blankmail = Прототип {$blankMail} не існує. Щось дуже не так. Зверніться до розробника.
command-mailto-bogus-mail = {$blankMail} не мав {$requiredMailComponent}. Щось дуже не так. Зверніться до розробника.
command-mailto-invalid-container = Цільова сутність-контейнер не має контейнера {$requiredContainer}.
command-mailto-unable-to-receive = Цільову сутність-одержувача не вдалося налаштувати для отримання пошти. Можливо, відсутній ID.
command-mailto-no-teleporter-found = Цільову сутність-одержувача не вдалося зіставити з жодним поштовим телепортом станції. Одержувач може перебувати поза станцією.
command-mailto-success = Успіх! Посилку поставлено в чергу на наступний телепорт через {$timeToTeleport} секунд
command-mailnow = Примусити всі поштові телепорти доставити наступну партію пошти якомога швидше. Це не омине ліміт на недоставлену пошту.
command-mailnow-help = Використання: {$command}
command-mailnow-success = Успіх! Усі поштові телепорти незабаром доставлять наступну партію пошти
