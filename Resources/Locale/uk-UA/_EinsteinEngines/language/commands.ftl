command-list-langs-desc = Список мов, якими ваша поточна сутність може говорити в даний момент.
command-list-langs-help = Використання: {$command}

command-saylang-desc = Надіслати повідомлення певною мовою. Щоб вибрати мову, ви можете використати її назву або її позицію у списку мов.
command-saylang-help = Використання: {$command} <id мови> <повідомлення>. Приклад: {$command} TauCetiBasic "Привіт, Світе!". Приклад: {$command} 1 "Привіт, Світе!"

command-language-select-desc = Вибрати поточну мову вашої сутності. Ви можете використати назву мови або її позицію у списку мов.
command-language-select-help = Використання: {$command} <id мови>. Приклад: {$command} 1. Приклад: {$command} TauCetiBasic

command-language-spoken = Розмовляє:
command-language-understood = Розуміє:
command-language-current-entry = {$id}. {$language} - {$name} (поточна)
command-language-entry = {$id}. {$language} - {$name}

command-language-invalid-number = Номер мови має бути від 0 до {$total}. Альтернативно, використовуйте назву мови.
command-language-invalid-language = Мова {$id} не існує або ви не можете нею розмовляти.

# Toolshed

command-description-language-add = Додає нову мову до сутності в конвеєрі. Два останні аргументи вказують, чи має вона бути розмовною/зрозумілою. Приклад: 'self language:add "Canilunzt" true true'
command-description-language-rm = Видаляє мову з сутності в конвеєрі. Працює аналогічно до language:add. Приклад: 'self language:rm "TauCetiBasic" true true'.
command-description-language-lsspoken = Перераховує всі мови, якими сутність може говорити. Приклад: 'self language:lsspoken'
command-description-language-lsunderstood = Перераховує всі мови, які сутність може розуміти. Приклад: 'self language:lssunderstood'

command-description-translator-addlang = Додає нову цільову мову до сутності-перекладача в конвеєрі. Дивіться language:add для деталей.
command-description-translator-rmlang = Видаляє цільову мову з сутності-перекладача в конвеєрі. Дивіться language:rm для деталей.
command-description-translator-addrequired = Додає нову необхідну мову до сутності-перекладача в конвеєрі. Приклад: 'ent 1234 translator:addrequired "TauCetiBasic"'
command-description-translator-rmrequired = Видаляє необхідну мову з сутності-перекладача в конвеєрі. Приклад: 'ent 1234 translator:rmrequired "TauCetiBasic"'
command-description-translator-lsspoken = Перераховує всі розмовні мови для сутності-перекладача в конвеєрі. Приклад: 'ent 1234 translator:lsspoken'
command-description-translator-lsunderstood = Перераховує всі зрозумілі мови для сутності-перекладача в конвеєрі. Приклад: 'ent 1234 translator:lssunderstood'
command-description-translator-lsrequired = Перераховує всі необхідні мови для сутності-перекладача в конвеєрі. Приклад: 'ent 1234 translator:lsrequired'

command-language-error-this-will-not-work = Це не спрацює.
command-language-error-not-a-translator = Сутність {$entity} не є перекладачем.
