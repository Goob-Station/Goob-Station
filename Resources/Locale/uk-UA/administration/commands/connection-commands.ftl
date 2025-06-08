## Strings for the "grant_connect_bypass" command.

cmd-grant_connect_bypass-desc = Тимчасово дозволити користувачеві обійти регулярні перевірки з'єднання.
cmd-grant_connect_bypass-help = Використання: grant_connect_bypass <користувач> [тривалість хвилин].
    Тимчасово надає користувачеві можливість обійти обмеження на звичайні з'єднання.
    Обхід застосовується лише до цього ігрового сервера і закінчується через (за замовчуванням) 1 годину.
    Користувач зможе приєднатися незалежно від білого списку, панічного бункера або ліміту гравця.

cmd-grant_connect_bypass-arg-user = <користувач>
cmd-grant_connect_bypass-arg-duration = [тривалість хвилини]

cmd-grant_connect_bypass-invalid-args = Очікувані 1 або 2 аргументи
cmd-grant_connect_bypass-unknown-user = Не вдалося знайти користувача '{$user}'
cmd-grant_connect_bypass-invalid-duration = Неправильна тривалість '{$duration}'

cmd-grant_connect_bypass-success = Успішно додано обхід для користувача '{$user}'
