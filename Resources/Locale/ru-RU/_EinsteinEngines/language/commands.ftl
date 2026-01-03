command-list-langs-desc = Показывает языки, на которых ваша текущая сущность может говорить в данный момент.
command-list-langs-help = Использование: { $command }
command-saylang-desc = Отправляет сообщение на выбранном языке. Для выбора языка можно использовать его название или номер в списке языков.
command-saylang-help = Использование: { $command } <language id> <message>. Пример: { $command } TauCetiBasic "Hello World!". Пример: { $command } 1 "Привет Мир!"
command-language-select-desc = Выбирает текущий язык общения вашей сущности. Можно указать название языка или его номер в списке.
command-language-select-help = Использование: { $command } <language id>. Пример: { $command } 1. Пример: { $command } TauCetiBasic
command-language-spoken = Говорит:
command-language-understood = Понимает:
command-language-current-entry = { $id }. { $language } - { $name } (current)
command-language-entry = { $id }. { $language } - { $name }
command-language-invalid-number = Номер языка должен быть от 0 до { $total }. Либо используйте название языка.
command-language-invalid-language = Язык { $id } не существует или вы не можете на нем говорить.

# Toolshed

command-description-language-add = Добавляет новый язык к выбранной сущности. Последние два аргумента указывают, будет ли язык выговариваемым/понимаемым. Пример: 'self language:add "Canilunzt" true true'
command-description-language-rm = Удаляет язык у выбранной сущности. Работает аналогично language:add. Пример: 'self language:rm "TauCetiBasic" true true'.
command-description-language-lsspoken = Показывает все выговариваемые языки сущности. Пример: 'self language:lsspoken'
command-description-language-lsunderstood = Показывает все понимаемые языки сущности. Пример: 'self language:lssunderstood'
command-description-translator-addlang = Добавляет новый целевой язык у выбранного переводчика. Смотри language:add for details.
command-description-translator-rmlang = Удаляет целевой язык у выбранного переводчика. Смотри language:rm for details.
command-description-translator-addrequired = Добавляет новый обязательный язык у выбранного переводчика. Пример:: 'ent 1234 translator:addrequired "TauCetiBasic"'
command-description-translator-rmrequired = Удаляет обязательный язык у выбранного переводчика. Пример: 'ent 1234 translator:rmrequired "TauCetiBasic"'
command-description-translator-lsspoken = Показывает все говоримые языки у выбранного переводчика. Пример: 'ent 1234 translator:lsspoken'
command-description-translator-lsunderstood = Показывает все понимаемые языки у выбранного переводчика. Пример: 'ent 1234 translator:lssunderstood'
command-description-translator-lsrequired = Показывает все обязательные языки у выбранного переводчика. Пример: 'ent 1234 translator:lsrequired'
command-language-error-this-will-not-work = Это не сработает.
command-language-error-not-a-translator = Сущность { $entity } не является переводчиком.
