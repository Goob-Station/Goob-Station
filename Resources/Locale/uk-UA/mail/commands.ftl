# Mailto
command-mailto-description = Поставити посилку в чергу на доставку до сутності. Приклад використання: `mailto 1234 5678 false false`. Вміст цільового контейнера буде перенесено у справжню поштову посилку.
### Frontier: додати опис is-large
command-mailto-help = Використання: {$command} <recipient entityUid> <container entityUid> [is-fragile: true або false] [is-priority: true або false] [is-large: true або false, опціонально]
command-mailto-no-mailreceiver = Цільова сутність-одержувач не має {$requiredComponent}.
command-mailto-no-blankmail = Прототип {$blankMail} не існує. Щось пішло не так. Зв'яжіться з програмістом.
command-mailto-bogus-mail = {$blankMail} не мав {$requiredMailComponent}. Щось пішло не так. Зв'яжіться з програмістом.
command-mailto-invalid-container = Цільова сутність-контейнер не має контейнера {$requiredContainer}.
command-mailto-unable-to-receive = Не вдалося налаштувати цільову сутність-одержувача для отримання пошти. Можливо, відсутній ID.
command-mailto-no-teleporter-found = Не вдалося знайти відповідний поштовий телепортер станції для цільової сутності-одержувача. Можливо, одержувач знаходиться поза станцією.
command-mailto-success = Успіх! Поштова посилка поставлена в чергу на наступну телепортацію через {$timeToTeleport} секунд
command-mailnow = Примусово змусити всі поштові телепортери доставити наступну партію пошти якомога швидше. Це не обійде ліміт недоставленої пошти.
command-mailnow-help = Використання: {$command}
command-mailnow-success = Успіх! Усі поштові телепортери незабаром доставлять наступну партію пошти
command-mailtestbulk = Надсилає по одному примірнику кожного типу посилки на вказаний поштовий телепортер. Неявно викликає mailnow.
command-mailtestbulk-help = Використання: {$command} <id_телепортера>
command-mailtestbulk-success = Успіх! Усі поштові телепортери незабаром доставлять ще одну партію пошти.
