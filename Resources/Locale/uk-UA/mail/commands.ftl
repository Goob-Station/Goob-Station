# Mailto
command-mailto-description = Черга посилок для доставки об'єкту. Приклад використання: `mailto 1234 5678 false false`. Вміст цільового контейнера буде перенесено в реальну поштову посилку.
command-mailto-help = Використання: {$command} <отримувач entityUid> <контейнер entityUid> [крихке: true або false] [пріоритетне: true або false] [велике: true або false, необов'язково]
command-mailto-no-mailreceiver = Цільовий отримувач не має {$requiredComponent}.
command-mailto-no-blankmail = Прототип {$blankMail} не існує. Щось дуже не так. Зверніться до програміста.
command-mailto-bogus-mail = {$blankMail} не мав {$requiredMailComponent}. Щось дуже не так. Зверніться до програміста.
command-mailto-invalid-container = Цільовий контейнер не має потрібного контейнера {$requiredContainer}.
command-mailto-unable-to-receive = Цільовий отримувач не зміг прийняти пошту. Можлива відсутність ID.
command-mailto-no-teleporter-found = Цільовий отримувач не відповідає жодному поштовому телепортеру станції. Можливо, отримувач знаходиться за межами станції.
command-mailto-success = Успіх! Поштова посилка поставлена в чергу на телепортацію через {$timeToTeleport} секунд.

# Mailnow
command-mailnow = Примусово змусити всі поштові телепортери здійснити наступну доставку пошти якнайшвидше. Це не перевищить ліміт недоставленої пошти.
command-mailnow-help = Використання: {$command}
command-mailnow-success = Успіх! Усі поштові телепортери незабаром здійснять чергову доставку пошти.

# Mailtestbulk
command-mailtestbulk = Надсилає один з кожного типу посилки на вказаний поштовий телепортер. Неявно викликає команду mailnow.
command-mailtestbulk-help = Використання: {$command} <teleporter_id>
command-mailtestbulk-success = Успіх! Усі поштові телепортери незабаром здійснять чергову доставку пошти.
