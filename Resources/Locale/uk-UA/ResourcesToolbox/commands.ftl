### Localization for engine console commands

## generic command errors

cmd-invalid-arg-number-error = Невірна кількість аргументів.

cmd-parse-failure-integer = {$arg} не є дійсним цілим числом.
cmd-parse-failure-float = {$arg} не є дійсним числом з плаваючою комою.
cmd-parse-failure-bool = {$arg} не є дійсним булевим значенням.
cmd-parse-failure-uid = {$arg} не є дійсним UID сутності.
cmd-parse-failure-mapid = {$arg} не є дійсним MapId.
cmd-parse-failure-enum = {$arg} не є переліком {$enum}.
cmd-parse-failure-grid = {$arg} не є дійсною сіткою.
cmd-parse-failure-cultureinfo = """{$arg}"" не є дійсним CultureInfo.
cmd-parse-failure-entity-exist = UID {$arg} не відповідає існуючій сутності.
cmd-parse-failure-session = Немає сесії з ім'ям користувача: {$username}

cmd-error-file-not-found = Не вдалося знайти файл: {$file}.
cmd-error-dir-not-found = Не вдалося знайти каталог: {$dir}.

cmd-failure-no-attached-entity = Немає сутності, прикріпленої до цієї оболонки
cmd-help-desc = Відобразити загальну довідку або довідку для конкретної команди
cmd-help-help = Використання: help [назва команди]
    Якщо назва команди не вказана, відображається загальна довідка. Якщо назва команди вказана, відображається довідка для цієї команди.

cmd-help-no-args = Щоб відобразити довідку для конкретної команди, напишіть 'help <команда>'. Щоб переглянути всі доступні команди, напишіть 'list'. Щоб шукати команди, використовуйте 'list <фільтр>'
cmd-help-unknown = Невідома команда: { $command }
cmd-help-top = { $command } - { $description }
cmd-help-invalid-args = Невірна кількість аргументів.
cmd-help-arg-cmdname = [назва команди]
cmd-cvar-desc = Отримує або встановлює CVar.
cmd-cvar-help = Використання: cvar <назва | ?> [значення]
    Якщо передано значення, воно розбирається та зберігається як нове значення CVar.
    Якщо ні, відображається поточне значення CVar.
    Використовуйте 'cvar ?', щоб отримати список усіх зареєстрованих CVar.

cmd-cvar-invalid-args = Необхідно надати один або два аргументи.
cmd-cvar-not-registered = CVar '{ $cvar }' не зареєстровано. Використовуйте 'cvar ?', щоб отримати список усіх зареєстрованих CVar.
cmd-cvar-parse-error = Вхідне значення має невірний формат для типу { $type }
cmd-cvar-compl-list = Список доступних CVars
cmd-cvar-arg-name = <назва | ?>
cmd-cvar-value-hidden = <значення приховано>
cmd-cvar_subs-desc = Перелічує підписки OnValueChanged для CVar.
cmd-cvar_subs-help = Usage: cvar_subs <name>

cmd-cvar_subs-invalid-args = Must provide exactly one argument.
cmd-cvar_subs-arg-name = <name>

## 'list' command
cmd-list-desc = Перераховує доступні команди з необов'язковим фільтром пошуку
cmd-list-help = Використання: list [фільтр]
    Перераховує всі доступні команди. Якщо вказано аргумент, він буде використовуватися для фільтрації команд за назвою.

cmd-list-heading = СТОРОНА НАЗВА          ОПИС{""\u000A""}-------------------------{""\u000A""}

cmd-list-arg-filter = [фільтр]
cmd-remoteexec-desc = Виконує команди на стороні сервера
cmd-remoteexec-help = Використання: > <команда> [аргумент] [аргумент] [аргумент...]
    Виконує команду на сервері. Це необхідно, якщо команда з такою ж назвою існує на клієнті, оскільки просте виконання команди спочатку запустить клієнтську команду
cmd-gc-desc = Запустити GC (Збирач сміття)
cmd-gc-help = Використання: gc [покоління]
    Використовує GC.Collect() для запуску збирача сміття.
    Якщо надано аргумент, він розбирається як номер покоління GC, і використовується GC.Collect(int).
    Використовуйте команду 'gfc' для повного збирання сміття з ущільненням LOH.
cmd-gc-failed-parse = Не вдалося розібрати аргумент.
cmd-gc-arg-generation = [покоління]
cmd-gcf-desc = Запустити GC повністю, з ущільненням LOH і всього іншого.
cmd-gcf-help = Використання: gcf
    Виконує повне GC.Collect(2, GCCollectionMode.Forced, true, true), а також ущільнює LOH.
    Це, ймовірно, заблокує систему на сотні мілісекунд, будьте обережні
cmd-gc_mode-desc = Змінити/Прочитати режим затримки GC
cmd-gc_mode-help = Використання: gc_mode [тип]
    Якщо аргумент не надано, повертає поточний режим затримки GC.
    Якщо аргумент передано, він розбирається як GCLatencyMode і встановлюється як режим затримки GC.

cmd-gc_mode-current = поточний режим затримки gc: { $prevMode }
cmd-gc_mode-possible = можливі режими:
cmd-gc_mode-option = - { $mode }
cmd-gc_mode-unknown = невідомий режим затримки gc: { $arg }
cmd-gc_mode-attempt = спроба змінити режим затримки gc: { $prevMode } -> { $mode }
cmd-gc_mode-result = результуючий режим затримки gc: { $mode }
cmd-gc_mode-arg-type = [тип]
cmd-mem-desc = Виводить інформацію про керовану пам'ять
cmd-mem-help = Використання: mem

cmd-mem-report = Розмір купи: { TOSTRING($heapSize, ""N0"") }
    Загалом виділено: { TOSTRING($totalAllocated, ""N0"") }
cmd-physics-overlay = {$overlay} не є розпізнаним накладенням
cmd-lsasm-desc = Перераховує завантажені збірки за контекстом завантаження
cmd-lsasm-help = Використання: lsasm
cmd-exec-desc = Виконує файл скрипта з директорії користувача гри, доступної для запису
cmd-exec-help = Використання: exec <назваФайлу>
    Кожен рядок у файлі виконується як окрема команда, якщо він не починається з #

cmd-exec-arg-filename = <назваФайлу>
cmd-dump_net_comps-desc = Друкує таблицю мережевих компонентів.
cmd-dump_net_comps-help = Використання: dump_net-comps

cmd-dump_net_comps-error-writeable = Реєстрація все ще доступна для запису, мережеві ідентифікатори не були згенеровані.
cmd-dump_net_comps-header = Реєстрації мережевих компонентів:
cmd-dump_event_tables-desc = Друкує таблиці спрямованих подій для сутності.
cmd-dump_event_tables-help = Використання: dump_event_tables <uid сутності>

cmd-dump_event_tables-missing-arg-entity = Відсутній аргумент сутності
cmd-dump_event_tables-error-entity = Недійсна сутність
cmd-dump_event_tables-arg-entity = <uid сутності>
cmd-monitor-desc = Перемикає монітор налагодження в меню F3.
cmd-monitor-help = Використання: monitor <назва>
    Можливі монітори: { $monitors }
    Ви також можете використовувати спеціальні значення ""-all"" та ""+all"", щоб приховати або показати всі монітори відповідно.

cmd-monitor-arg-monitor = <монітор>
cmd-monitor-invalid-name = Недійсна назва монітора
cmd-monitor-arg-count = Відсутній аргумент монітора
cmd-monitor-minus-all-hint = Приховує всі монітори
cmd-monitor-plus-all-hint = Показує всі монітори
cmd-set-ambient-light-desc = Дозволяє встановити навколишнє освітлення для вказаної карти в SRGB.
cmd-set-ambient-light-help = setambientlight [id карти] [r g b a]
cmd-set-ambient-light-parse = Не вдалося розібрати аргументи як байтові значення для кольору

cmd-savemap-desc = Серіалізує карту на диск. Не зберігатиме карту після ініціалізації, якщо не змусити.
cmd-savemap-help = savemap <ID карти> <Шлях> [force]
cmd-savemap-not-exist = Цільова карта не існує.
cmd-savemap-init-warning = Спроба зберегти карту після ініціалізації без примусового збереження.
cmd-savemap-attempt = Спроба зберегти карту {$mapId} до {$path}.
cmd-savemap-success = Карту успішно збережено.
cmd-savemap-error = Could not save map! See server log for details.
cmd-hint-savemap-id = <MapID>
cmd-hint-savemap-path = <Шлях>
cmd-hint-savemap-force = [bool]

cmd-loadmap-desc = Завантажує карту з диска в гру.
cmd-loadmap-help = loadmap <ID карти> <Шлях> [x] [y] [rotation] [consistentUids]
cmd-loadmap-nullspace = Ви не можете завантажити в карту 0.
cmd-loadmap-exists = Карта {$mapId} вже існує.
cmd-loadmap-success = Карту {$mapId} було завантажено з {$path}.
cmd-loadmap-error = Сталася помилка під час завантаження карти з {$path}.
cmd-hint-loadmap-x-position = [x-позиція]
cmd-hint-loadmap-y-position = [y-позиція]
cmd-hint-loadmap-rotation = [обертання]
cmd-hint-loadmap-uids = [float]

cmd-hint-savebp-id = <ID сутності сітки>

cmd-flushcookies-desc = Зберегти сховище файлів cookie CEF на диск
cmd-flushcookies-help = Це гарантує, що файли cookie будуть правильно збережені на диску в разі некоректного завершення роботи.
    Зверніть увагу, що фактична операція є асинхронною.

cmd-ldrsc-desc = Попередньо кешує ресурс.
cmd-ldrsc-help = Використання: ldrsc <шлях> <тип>

cmd-rldrsc-desc = Перезавантажує ресурс.
cmd-rldrsc-help = Використання: rldrsc <шлях> <тип>

cmd-gridtc-desc = Отримує кількість плиток сітки.
cmd-gridtc-help = Використання: gridtc <id сітки>
cmd-guidump-desc = Зберегти дерево GUI у /guidump.txt в даних користувача.
cmd-guidump-help = Використання: guidump

cmd-uitest-desc = Відкрити фіктивне вікно для тестування UI
cmd-uitest-help = Використання: uitest
cmd-uitest2-desc = Відкриває вікно ОС для тестування елементів керування UI
cmd-uitest2-help = Використання: uitest2 <вкладка>
cmd-uitest2-arg-tab = <вкладка>
cmd-uitest2-error-args = Очікувався щонайбільше один аргумент
cmd-uitest2-error-tab = Недійсна вкладка: '{$value}'
cmd-uitest2-title = UITest2


cmd-setclipboard-desc = Встановлює системний буфер обміну
cmd-setclipboard-help = Використання: setclipboard <текст>

cmd-getclipboard-desc = Отримує системний буфер обміну
cmd-getclipboard-help = Використання: Getclipboard

cmd-togglelight-desc = Перемикає рендеринг світла.
cmd-togglelight-help = Використання: togglelight

cmd-togglefov-desc = Перемикає поле зору для клієнта.
cmd-togglefov-help = Використання: togglefov

cmd-togglehardfov-desc = Перемикає жорстке поле зору для клієнта. (для налагодження space-station-14#2353)
cmd-togglehardfov-help = Використання: togglehardfov

cmd-toggleshadows-desc = Перемикає рендеринг тіней.
cmd-toggleshadows-help = Використання: toggleshadows

cmd-togglelightbuf-desc = Перемикає рендеринг освітлення. Це включає тіні, але не поле зору.
cmd-togglelightbuf-help = Використання: togglelightbuf

cmd-chunkinfo-desc = Отримує інформацію про чанк під курсором миші.
cmd-chunkinfo-help = Використання: chunkinfo

cmd-rldshader-desc = Перезавантажує всі шейдери.
cmd-rldshader-help = Використання: rldshader

cmd-cldbglyr-desc = Перемкнути шари налагодження поля зору та світла.
cmd-cldbglyr-help = Використання: cldbglyr <шар>: Перемкнути <шар>
    cldbglyr: Вимкнути всі шари

cmd-key-info-desc = Інформація про клавішу.
cmd-key-info-help = Використання: keyinfo <Key>

## 'bind' command
cmd-bind-desc = Прив'язує комбінацію клавіш до команди.
cmd-bind-help = Використання: bind { cmd-bind-arg-key } { cmd-bind-arg-mode } { cmd-bind-arg-command }
    Зауважте, що це НЕ зберігає прив'язки автоматично.
    Використовуйте команду 'svbind', щоб зберегти конфігурацію прив'язок.

cmd-bind-arg-key = <НазваКлавіші>
cmd-bind-arg-mode = <РежимПрив'язки>
cmd-bind-arg-command = <КомандаВводу>

cmd-net-draw-interp-desc = Перемикає налагоджувальне відображення мережевої інтерполяції.
cmd-net-draw-interp-help = Використання: net_draw_interp

cmd-net-watch-ent-desc = Виводить усі мережеві оновлення для EntityId у консоль.
cmd-net-watch-ent-help = Використання: net_watchent <0|EntityUid>

cmd-net-refresh-desc = Запитує повний стан сервера.
cmd-net-refresh-help = Використання: net_refresh

cmd-net-entity-report-desc = Перемикає панель звітів про мережеві сутності.
cmd-net-entity-report-help = Використання: net_entityreport

cmd-fill-desc = Заповнює консоль для налагодження.
cmd-fill-help = Заповнює консоль різним сміттям для налагодження.

cmd-cls-desc = Очищує консоль.
cmd-cls-help = Очищує консоль налагодження від усіх повідомлень.

cmd-sendgarbage-desc = Надсилає сміття на сервер.
cmd-sendgarbage-help = Сервер відповість 'no u'

cmd-loadgrid-desc = Завантажує грід із файлу на існуючу мапу.
cmd-loadgrid-help = loadgrid <ID_Мапи> <Шлях> [x y] [обертання] [storeUids]

cmd-loc-desc = Виводить абсолютне місцезнаходження сутності гравця в консоль.
cmd-loc-help = loc

cmd-tpgrid-desc = Телепортує грід у нове місце.
cmd-tpgrid-help = tpgrid <ID_гріду> <X> <Y> [<ID_Мапи>]

cmd-rmgrid-desc = Видаляє грід з мапи. Ви не можете видалити стандартний грід.
cmd-rmgrid-help = rmgrid <ID_гріду>

cmd-mapinit-desc = Запускає ініціалізацію мапи.
cmd-mapinit-help = mapinit <ID_мапи>

cmd-lsmap-desc = Виводить список мап.
cmd-lsmap-help = lsmap

cmd-lsgrid-desc = Виводить список грідів.
cmd-lsgrid-help = lsgrid

cmd-addmap-desc = Додає нову порожню мапу до раунду. Якщо ID мапи вже існує, ця команда нічого не робить.
cmd-addmap-help = addmap <ID_мапи> [pre-init]

cmd-rmmap-desc = Видаляє мапу зі світу. Ви не можете видалити нуль-простір.
cmd-rmmap-help = rmmap <ID_мапи>

cmd-savegrid-desc = Серіалізує грід на диск.
cmd-savegrid-help = savegrid <ID_гріду> <Шлях>

cmd-testbed-desc = Завантажує тестовий стенд фізики на вказану мапу.
cmd-testbed-help = testbed <id_мапи> <тест>

## Команда 'flushcookies'
# Примітка: команда flushcookies належить до Robust.Client.WebView, її немає в основному коді рушія.

## Команда 'addcomp'
cmd-addcomp-desc = Додає компонент до сутності.
cmd-addcomp-help = addcomp <uid> <назва_компонента>
cmd-addcompc-desc = Додає компонент до сутності на клієнті.
cmd-addcompc-help = addcompc <uid> <назва_компонента>

## Команда 'rmcomp'
cmd-rmcomp-desc = Видаляє компонент із сутності.
cmd-rmcomp-help = rmcomp <uid> <назва_компонента>
cmd-rmcompc-desc = Видаляє компонент із сутності на клієнті.
cmd-rmcompc-help = rmcomp <uid> <назва_компонента>

## Команда 'addview'
cmd-addview-desc = Дозволяє підписатися на перегляд сутності для цілей налагодження.
cmd-addview-help = addview <entityUid>
cmd-addviewc-desc = Дозволяє підписатися на перегляд сутності для цілей налагодження.
cmd-addviewc-help = addview <entityUid>

## Команда 'removeview'
cmd-removeview-desc = Дозволяє відписатися від перегляду сутності для цілей налагодження.
cmd-removeview-help = removeview <entityUid>

## Команда 'loglevel'
cmd-loglevel-desc = Змінює рівень логування для вказаного sawmill.
cmd-loglevel-help = Використання: loglevel <sawmill> <level>
      sawmill: Префікс мітки для повідомлень логу. Це те, для чого ви встановлюєте рівень.
      level: Рівень логування. Має відповідати одному зі значень переліку LogLevel.

cmd-testlog-desc = Записує тестовий лог у sawmill.
cmd-testlog-help = Використання: testlog <sawmill> <level> <message>
    sawmill: Префікс мітки для повідомлення, що логується.
    level: Рівень логування. Має відповідати одному зі значень переліку LogLevel.
    message: Повідомлення, яке буде залоговано. Візьміть його в подвійні лапки, якщо хочете використовувати пробіли.

## Команда 'vv'
cmd-vv-desc = Відкриває Перегляд Змінних.
cmd-vv-help = Використання: vv <ID сутності|назва інтерфейсу IoC|назва інтерфейсу SIoC>

## Команда 'showvelocities'
cmd-showvelocities-desc = Показує вашу кутову та лінійну швидкості.
cmd-showvelocities-help = Використання: showvelocities

## Команда 'setinputcontext'
cmd-setinputcontext-desc = Встановлює активний контекст вводу.
cmd-setinputcontext-help = Використання: setinputcontext <контекст>

## Команда 'forall'
cmd-forall-desc = Виконує команду для всіх сутностей з вказаним компонентом.
cmd-forall-help = Використання: forall <bql запит> do <команда...>

## Команда 'delete'
cmd-delete-desc = Видаляє сутність із вказаним ID.
cmd-delete-help = delete <UID сутності>

# Системні команди
cmd-showtime-desc = Показує час сервера.
cmd-showtime-help = showtime

cmd-restart-desc = Коректно перезапускає сервер (а не тільки раунд).
cmd-restart-help = restart

cmd-shutdown-desc = Коректно вимикає сервер.
cmd-shutdown-help = shutdown

cmd-saveconfig-desc = Зберігає конфігурацію сервера у файл конфігурації.
cmd-saveconfig-help = saveconfig

cmd-netaudit-desc = Виводить інформацію про безпеку NetMsg.
cmd-netaudit-help = netaudit

# Команди гравця
cmd-tp-desc = Телепортує гравця в будь-яке місце в раунді.
cmd-tp-help = tp <x> <y> [<ID_мапи>]

cmd-tpto-desc = Телепортує поточного гравця або вказаних гравців/сутностей до місця розташування першого гравця/сутності.
cmd-tpto-help = tpto <ім'я_користувача|uid> [ім'я_користувача|NetEntity]...
cmd-tpto-destination-hint = пункт призначення (NetEntity або ім'я користувача)
cmd-tpto-victim-hint = сутність для телепортації (NetEntity або ім'я користувача)
cmd-tpto-parse-error = Неможливо визначити сутність або гравця: {$str}

cmd-listplayers-desc = Виводить список усіх підключених гравців.
cmd-listplayers-help = listplayers

cmd-kick-desc = Викидає підключеного гравця з сервера, відключаючи його.
cmd-kick-help = kick <ІндексГравця> [<Причина>]

# Команда Spin
cmd-spin-desc = Змушує сутність обертатися. За замовчуванням сутність — батьківський об'єкт гравця.
cmd-spin-help = spin швидкість [опір] [entityUid]

# Команда локалізації
cmd-rldloc-desc = Перезавантажує локалізацію (клієнт і сервер).
cmd-rldloc-help = Використання: rldloc

# Керування налагодженням сутностей
cmd-spawn-desc = Створює сутність певного типу.
cmd-spawn-help = spawn <прототип> АБО spawn <прототип> <відносний ID сутності> АБО spawn <прототип> <x> <y>
cmd-cspawn-desc = Створює клієнтську сутність певного типу біля ваших ніг.
cmd-cspawn-help = cspawn <тип сутності>

cmd-scale-desc = Наївно збільшує або зменшує розмір сутності.
cmd-scale-help = scale <entityUid> <число>

cmd-dumpentities-desc = Вивести список сутностей.
cmd-dumpentities-help = Виводить список сутностей з їх UID та прототипами.

cmd-getcomponentregistration-desc = Отримує інформацію про реєстрацію компонента.
cmd-getcomponentregistration-help = Використання: getcomponentregistration <назва_компонента>

cmd-showrays-desc = Перемикає налагоджувальне відображення променів фізики. Необхідно вказати ціле число для <часу_життя_променя>.
cmd-showrays-help = Використання: showrays <час_життя_променя>

cmd-disconnect-desc = Негайно відключитися від сервера і повернутися в головне меню.
cmd-disconnect-help = Використання: disconnect

cmd-entfo-desc = Відображає детальну діагностику для сутності.
cmd-entfo-help = Використання: entfo <uid_сутності>
    UID сутності може мати префікс 'c', щоб перетворити його на UID клієнтської сутності.

cmd-fuck-desc = Викликає виняток
cmd-fuck-help = Використання: fuck

cmd-showpos-desc = Показати позицію всіх сутностей на екрані.
cmd-showpos-help = Використання: showpos

cmd-showrot-desc = Show the rotation of all entities on the screen.
cmd-showrot-help = Usage: showrot

cmd-showvel-desc = Show the local velocity of all entites on the screen.
cmd-showvel-help = Usage: showvel

cmd-showangvel-desc = Show the angular velocity of all entities on the screen.
cmd-showangvel-help = Usage: showangvel

cmd-sggcell-desc = Виводить список сутностей у клітинці сітки прив'язки.
cmd-sggcell-help = Використання: sggcell <ID_гріду> <vector2i>\nЦей параметр vector2i має формат x<int>,y<int>.

cmd-overrideplayername-desc = Змінює ім'я, що використовується при спробі підключення до сервера.
cmd-overrideplayername-help = Використання: overrideplayername <ім'я>

cmd-showanchored-desc = Показує закріплені сутності на певній плитці
cmd-showanchored-help = Використання: showanchored

cmd-dmetamem-desc = Виводить члени типу у форматі, придатному для файлу конфігурації пісочниці.
cmd-dmetamem-help = Використання: dmetamem <тип>

cmd-launchauth-desc = Завантажує токени автентифікації з даних лаунчера для допомоги в тестуванні активних серверів.
cmd-launchauth-help = Використання: launchauth <назва_акаунту>

cmd-lightbb-desc = Перемикає відображення обмежувальних рамок світла.
cmd-lightbb-help = Використання: lightbb

cmd-monitorinfo-desc = Інформація про монітори
cmd-monitorinfo-help = Використання: monitorinfo <id>

cmd-setmonitor-desc = Встановити монітор
cmd-setmonitor-help = Використання: setmonitor <id>

cmd-physics-desc = Показує налагоджувальний оверлей фізики. Аргумент вказує оверлей.
cmd-physics-help = Використання: physics <aabbs / com / contactnormals / contactpoints / distance / joints / shapeinfo / shapes>

cmd-hardquit-desc = Миттєво закриває ігровий клієнт.
cmd-hardquit-help = Миттєво закриває ігровий клієнт, не залишаючи слідів. Без прощання з сервером.

cmd-quit-desc = Коректно закриває ігровий клієнт.
cmd-quit-help = Правильно закриває ігровий клієнт, сповіщаючи підключений сервер і т.д.

cmd-csi-desc = Відкриває інтерактивну консоль C#.
cmd-csi-help = Використання: csi

cmd-scsi-desc = Відкриває інтерактивну консоль C# на сервері.
cmd-scsi-help = Використання: scsi

cmd-watch-desc = Відкриває вікно спостереження за змінними.
cmd-watch-help = Використання: watch

cmd-showspritebb-desc = Перемикає відображення меж спрайтів
cmd-showspritebb-help = Використання: showspritebb

cmd-togglelookup-desc = Показує / приховує межі пошуку сутностей через оверлей.
cmd-togglelookup-help = Використання: togglelookup

cmd-net_entityreport-desc = Перемикає панель звітів про мережеві сутності.
cmd-net_entityreport-help = Використання: net_entityreport

cmd-net_refresh-desc = Запитує повний стан сервера.
cmd-net_refresh-help = Використання: net_refresh

cmd-net_graph-desc = Перемикає панель статистики мережі.
cmd-net_graph-help = Використання: net_graph

cmd-net_watchent-desc = Виводить усі мережеві оновлення для EntityId у консоль.
cmd-net_watchent-help = Використання: net_watchent <0|EntityUid>

cmd-net_draw_interp-desc = Перемикає налагоджувальне відображення мережевої інтерполяції.
cmd-net_draw_interp-help = Використання: net_draw_interp <0|EntityUid>

cmd-vram-desc = Відображає статистику використання відеопам'яті грою.
cmd-vram-help = Використання: vram

cmd-showislands-desc = Показує поточні фізичні тіла, задіяні в кожному фізичному острові.
cmd-showislands-help = Використання: showislands

cmd-showgridnodes-desc = Показує вузли для цілей розділення гріду.
cmd-showgridnodes-help = Використання: showgridnodes

cmd-profsnap-desc = Зробити знімок профілювання.
cmd-profsnap-help = Використання: profsnap

cmd-devwindow-desc = Вікно розробника
cmd-devwindow-help = Використання: devwindow

cmd-scene-desc = Негайно змінює сцену/стан інтерфейсу.
cmd-scene-help = Використання: scene <назва_класу>

cmd-szr_stats-desc = Повідомити статистику серіалізатора.
cmd-szr_stats-help = Використання: szr_stats

cmd-hwid-desc = Повертає поточний HWID (HardWare ID).
cmd-hwid-help = Використання: hwid

cmd-vvread-desc = Отримати значення шляху за допомогою VV (Перегляд Змінних).
cmd-vvread-help = Використання: vvread <шлях>

cmd-vvwrite-desc = Змінити значення шляху за допомогою VV (Перегляд Змінних).
cmd-vvwrite-help = Використання: vvwrite <шлях>

cmd-vvinvoke-desc = Викликати/Запустити шлях з аргументами за допомогою VV.
cmd-vvinvoke-help = Використання: vvinvoke <шлях> [аргументи...]

cmd-dump_dependency_injectors-desc = Вивести кеш інжектора залежностей IoCManager.
cmd-dump_dependency_injectors-help = Використання: dump_dependency_injectors
cmd-dump_dependency_injectors-total-count = Загальна кількість: { $total }

cmd-dump_netserializer_type_map-desc = Вивести карту типів NetSerializer та хеш серіалізатора.
cmd-dump_netserializer_type_map-help = Використання: dump_netserializer_type_map

cmd-hub_advertise_now-desc = Негайно оголосити на головний сервер хабу
cmd-hub_advertise_now-help = Використання: hub_advertise_now

cmd-echo-desc = Повертає аргументи в консоль
cmd-echo-help = Використання: echo "<повідомлення>"

## Команда 'vfs_ls'
cmd-vfs_ls-desc = Вивести вміст каталогу у VFS.
cmd-vfs_ls-help = Використання: vfs_list <шлях>
    Приклад:
    vfs_list /Assemblies

cmd-vfs_ls-err-args = Потрібен рівно 1 аргумент.
cmd-vfs_ls-hint-path = <шлях>

cmd-reloadtiletextures-desc = Перезавантажує атлас текстур плиток, щоб дозволити гаряче перезавантаження спрайтів плиток
cmd-reloadtiletextures-help = Використання: reloadtiletextures

cmd-audio_length-desc = Показує тривалість аудіофайлу
cmd-audio_length-help = Використання: audio_length { cmd-audio_length-arg-file-name }
cmd-audio_length-arg-file-name = <назва файлу>

## PVS
cmd-pvs-override-info-desc = Виводить інформацію про будь-які перевизначення PVS, пов'язані з сутністю.
cmd-pvs-override-info-empty = Сутність {$nuid} не має перевизначень PVS.
cmd-pvs-override-info-global = Сутність {$nuid} має глобальне перевизначення.
cmd-pvs-override-info-clients = Сутність {$nuid} має перевизначення сесії для {$clients}.

cmd-localization_set_culture-desc = Set DefaultCulture for the client LocalizationManager
cmd-localization_set_culture-help = Usage: localization_set_culture <cultureName>
cmd-localization_set_culture-culture-name = <cultureName>
cmd-localization_set_culture-changed = Localization changed to { $code } ({ $nativeName } / { $englishName })
