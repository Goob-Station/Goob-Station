### Локалізація для команд консолі рушія

## Загальні помилки команд

cmd-invalid-arg-number-error = Неправильна кількість аргументів.

cmd-parse-failure-integer = {$arg} не є дійсним цілим числом.
cmd-parse-failure-float = {$arg} не є дійсним числом з плаваючою комою.
cmd-parse-failure-bool = {$arg} не є дійсним значенням типу bool.
cmd-parse-failure-uid = {$arg} не є дійсним UID сутності.
cmd-parse-failure-mapid = {$arg} не є дійсним значенням MapId.
cmd-parse-failure-grid = {$arg} не є дійсною сіткою.
cmd-parse-failure-entity-exist = UID {$arg} не відповідає існуючій сутності.
cmd-parse-failure-session = Не існує сеансу з іменем користувача: {$username}

cmd-error-file-not-found = Не вдалося знайти файл: {$file}.
cmd-error-dir-not-found = Не вдалося знайти каталог: {$dir}.

cmd-failure-no-attached-entity = До цієї оболонки не прив'язано жодної сутності.

## Команда 'help'
cmd-help-desc = Відображає загальну довідку або текст довідки для конкретної команди
cmd-help-help = Використання: help [назва команди].
    Якщо назву команди не вказано, показує текст довідки загального призначення. Якщо вказано назву команди, показує текст довідки для цієї команди.

cmd-help-no-args = Щоб вивести довідку для певної команди, напишіть 'help <команда>'. Щоб вивести список усіх доступних команд, напишіть 'list'. Для пошуку команд використовуйте 'list <фільтр>'.
cmd-help-unknown = Невідома команда: {$command}
cmd-help-top = {$command} - {$description}
cmd-help-invalid-args = Неправильна кількість аргументів.
cmd-help-arg-cmdname = [назва команди]

## Команда 'cvar'
cmd-cvar-desc = Отримує або встановлює CVar.
cmd-cvar-help = Використання: cvar <ім'я | ?> [значення]
    Якщо передається значення, воно аналізується і зберігається як нове значення CVar.
    Якщо ні, відображається поточне значення CVar.
    Використовуйте 'cvar ?', щоб отримати список усіх зареєстрованих CVar.

cmd-cvar-invalid-args = Потрібно вказати один або два аргументи.
cmd-cvar-not-registered = CVar '{$cvar}' не зареєстровано. Використовуйте 'cvar ?', щоб отримати список усіх зареєстрованих CVar.
cmd-cvar-parse-error = Вхідне значення має неправильний формат для типу {$type}
cmd-cvar-compl-list = Перелік доступних CVar
cmd-cvar-arg-name = <ім'я | ?>
cmd-cvar-value-hidden = <значення приховано>

## Команда 'list'
cmd-list-desc = Перелік доступних команд з опціональним фільтром пошуку
cmd-list-help = Використання: list [фільтр].
    Виводить список усіх доступних команд. Якщо вказано аргумент, його буде використано для фільтрування команд за назвою.

cmd-list-heading = SIDE NAME DESC{"\u000A"}-------------------------{"\u000A"}

cmd-list-arg-filter = [фільтр]

## Команда '>', також відома як віддалене виконання
cmd-remoteexec-desc = Виконує команди на стороні сервера
cmd-remoteexec-help = Використання: > <команда> [аргумент] [аргумент] [аргумент...].
    Виконує команду на сервері. Це необхідно, якщо на клієнті існує команда з такою самою назвою, оскільки просте виконання команди призведе до виконання клієнтської команди.

## Команда 'gc'
cmd-gc-desc = Запускає збирач сміття (GC)
cmd-gc-help = Використання: gc [покоління].
    Використовує GC.Collect() для виконання збирача сміття.
    Якщо вказано аргумент, він буде інтерпретований як номер покоління GC і використовуватиметься в GC.Collect(int).
    Використовуйте команду 'gfc' для виконання LOH-компактуючого повного GC.
cmd-gc-failed-parse = Не вдалося розібрати аргумент.
cmd-gc-arg-generation = [покоління]

## Команда 'gcf'
cmd-gcf-desc = Запускає GC, повністю, з ущільненням LOH і всього іншого.
cmd-gcf-help = Використання: gcf.
    Виконує повний GC.Collect(2, GCCollectionMode.Forced, true, true), одночасно стискаючи LOH.
    Це, ймовірно, заблокує роботу на сотні мілісекунд, будьте уважні.

## Команда 'gc_mode'
cmd-gc_mode-desc = Змінює/зчитує режим затримки GC
cmd-gc_mode-help = Використання: gc_mode [тип].
    Якщо не вказано жодного аргументу, повертає поточний режим затримки GC.
    Якщо вказано аргумент, він буде інтерпретований як GCLatencyMode і встановлюватиметься як режим затримки GC.

cmd-gc_mode-current = поточний режим затримки GC: {$prevMode}
cmd-gc_mode-possible = можливі режими:
cmd-gc_mode-option = - {$mode}
cmd-gc_mode-unknown = невідомий режим затримки GC: {$arg}
cmd-gc_mode-attempt = спроба змінити режим затримки GC: {$prevMode} -> {$mode}
cmd-gc_mode-result = результуючий режим затримки GC: {$mode}
cmd-gc_mode-arg-type = [тип]

## Команда 'mem'
cmd-mem-desc = Друкує інформацію про керовану пам'ять
cmd-mem-help = Використання: mem

cmd-mem-report = Розмір купи: { TOSTRING($heapSize, "N0") }
    Всього виділено: { TOSTRING($totalAllocated, "N0") }

## Команда 'physics'
cmd-physics-overlay = {$overlay} не є розпізнаним оверлеєм.

## Команда 'lsasm'
cmd-lsasm-desc = Перелічує завантажені збірки за контекстом завантаження
cmd-lsasm-help = Використання: lsasm.

## Команда 'exec'
cmd-exec-desc = Виконує файл сценарію з доступних для запису даних користувача у грі
cmd-exec-help = Використання: exec <ім'я_файлу>.
    Кожен рядок у файлі виконується як окрема команда, якщо тільки він не починається з символу #

cmd-exec-arg-filename = <ім'я_файлу>

## Команда 'dump_net_comps'
cmd-dump_net_comps-desc = Виводить таблицю мережевих компонентів.
cmd-dump_net_comps-help = Використання: dump_net-comps

cmd-dump_net_comps-error-writeable = Реєстрація все ще доступна для запису, мережеві ідентифікатори ще не згенеровані.
cmd-dump_net_comps-header = Реєстрація мережевих компонентів:

## Команда 'dump_event_tables'
cmd-dump_event_tables-desc = Виводить таблиці подій для сутності.
cmd-dump_event_tables-help = Використання: dump_event_tables <entityUid>

cmd-dump_event_tables-missing-arg-entity = Відсутній аргумент сутності
cmd-dump_event_tables-error-entity = Неправильний UID сутності
cmd-dump_event_tables-arg-entity = <entityUid>

## Команда 'monitor'
cmd-monitor-desc = Перемикає режим моніторингу в меню F3.
cmd-monitor-help = Використання: monitor <назва>.
    Можливі монітори: {$monitors}.
    Ви також можете використовувати спеціальні значення "-all" і "+all", щоб приховати або показати всі монітори відповідно.

cmd-monitor-arg-monitor = <монітор>
cmd-monitor-invalid-name = Неправильна назва монітора
cmd-monitor-arg-count = Відсутній аргумент монітора
cmd-monitor-minus-all-hint = Приховує всі монітори
cmd-monitor-plus-all-hint = Показує всі монітори.

## Команда 'setambientlight'
cmd-set-ambient-light-desc = Дозволяє задати навколишнє освітлення для вказаної мапи у форматі SRGB.
cmd-set-ambient-light-help = використання: setambientlight [mapid] [r g b a]
cmd-set-ambient-light-parse = Неможливо розібрати аргументи у вигляді значень байтів для кольору.

## Команди відображення

cmd-savemap-desc = Зберігає мапу на диск. Не зберігає мапу після ініціалізації, якщо не примусово.
cmd-savemap-help = використання: savemap <MapID> <Шлях> [force]
cmd-savemap-not-exist = Цільової мапи не існує.
cmd-savemap-init-warning = Спроба зберегти мапу після ініціалізації без примусового збереження.
cmd-savemap-attempt = Спроба зберегти мапу {$mapId} до {$path}.
cmd-savemap-success = Мапу успішно збережено.
cmd-hint-savemap-id = <MapID>
cmd-hint-savemap-path = <Шлях>
cmd-hint-savemap-force = [bool]

cmd-loadmap-desc = Завантажує мапу з диска в гру.
cmd-loadmap-help = використання: loadmap <MapID> <Path> [x] [y] [rotation] [consistentUids]
cmd-loadmap-nullspace = Ви не можете завантажити мапу з ідентифікатором 0.
cmd-loadmap-exists = Мапа {$mapId} вже існує.
cmd-loadmap-success = Мапу {$mapId} було завантажено з {$path}.
cmd-loadmap-error = Виникла помилка під час завантаження мапи з {$path}.
cmd-hint-loadmap-x-position = [x-позиція]
cmd-hint-loadmap-y-position = [y-позиція]
cmd-hint-loadmap-rotation = [обертання]
cmd-hint-loadmap-uids = [плаває]

cmd-hint-savebp-id = <Ідентифікатор сітки>

## Команда 'flushcookies'
# Примітка: команда flushcookies походить з Robust.Client.WebView, її немає в основному коді рушія.
cmd-ldrsc-desc = Попередньо кешує ресурс.
cmd-ldrsc-help = Використання: ldrsc <шлях> <тип>

cmd-rldrsc-desc = Перезавантажує ресурс.
cmd-rldrsc-help = Використання: rldrsc <шлях> <тип>

cmd-gridtc-desc = Отримує кількість тайлів у сітці.
cmd-gridtc-help = Використання: gridtc <gridId>.

## Команди на стороні клієнта

cmd-guidump-desc = Вивантажує дерево графічного інтерфейсу у файл /guidump.txt у користувацьких даних.
cmd-guidump-help = Використання: guidump

cmd-uitest-desc = Відкриває вікно тестування інтерфейсу
cmd-uitest-help = Використання: uitest.

## Команда 'uitest2'
cmd-uitest2-desc = Відкриває вікно ОС для тестування інтерфейсу
cmd-uitest2-help = Використання: uitest2 <вкладка>
cmd-uitest2-arg-tab = <вкладка>
cmd-uitest2-error-args = Очікується не більше одного аргументу
cmd-uitest2-error-tab = Невірна вкладка: '{$value}'
cmd-uitest2-title = UITest2

cmd-setclipboard-desc = Встановлює значення системного буфера обміну
cmd-setclipboard-help = Використання: setclipboard <текст>

cmd-getclipboard-desc = Отримує значення системного буфера обміну
cmd-getclipboard-help = Використання: getclipboard

cmd-togglelight-desc = Перемикає рендеринг світла.
cmd-togglelight-help = Використання: togglelight

cmd-togglefov-desc = Перемикає FOV для клієнта.
cmd-togglefov-help = Використання: togglefov

cmd-togglehardfov-desc = Вмикає жорсткий FOV для клієнта. (для налагодження space-station-14#2353)
cmd-togglehardfov-help = Використання: togglehardfov

cmd-toggleshadows-desc = Перемикає рендеринг тіней.
cmd-toggleshadows-help = Використання: toggleshadows

cmd-togglelightbuf-desc = Перемикає рендеринг освітлення. Включає тіні, але не FOV.
cmd-togglelightbuf-help = Використання: togglelightbuf

cmd-chunkinfo-desc = Отримує інформацію про фрагмент під курсором миші.
cmd-chunkinfo-help = Використання: chunkinfo

cmd-rldshader-desc = Перезавантажує всі шейдери.
cmd-rldshader-help = Використання: rldshader

cmd-cldbglyr-desc = Перемикає налагоджувальні шари FOV та освітлення.
cmd-cldbglyr-help = Використання: cldbglyr <шар>: Перемкнути <шар>.
    cldbglyr: Вимкнути всі шари

cmd-key-info-desc = Отримує інформацію про клавішу.
cmd-key-info-help = Використання: keyinfo <Ключ>.

## Команда 'bind'
cmd-bind-desc = Призначає комбінацію клавіш для виконання команди.
cmd-bind-help = Використання: bind {cmd-bind-arg-key} {cmd-bind-arg-mode} {cmd-bind-arg-command}.
    Зверніть увагу, що це НЕ призведе до автоматичного збереження прив'язок.
    Для збереження конфігурації скористайтеся командою 'svbind'.

cmd-bind-arg-key = <НазваКлавіші>
cmd-bind-arg-mode = <РежимПрив'язки>
cmd-bind-arg-command = <КомандаВведення>

cmd-net-draw-interp-desc = Перемикає налагоджувальний малюнок мережевої інтерполяції.
cmd-net-draw-interp-help = Використання: net_draw_interp

cmd-net-watch-ent-desc = Виводить у консоль усі мережеві оновлення для EntityId.
cmd-net-watch-ent-help = Використання: net_watchent <0|EntityUid>

cmd-net-refresh-desc = Запитує повний стан сервера.
cmd-net-refresh-help = Використання: net_refresh

cmd-net-entity-report-desc = Перемикає панель звітів про мережеві сутності.
cmd-net-entity-report-help = Використання: net_entityreport

cmd-fill-desc = Заповнює консоль для налагодження.
cmd-fill-help = Заповнює консоль текстом для налагодження.

cmd-cls-desc = Очищає консоль.
cmd-cls-help = Очищає консоль налагодження від усіх повідомлень.

cmd-sendgarbage-desc = Надсилає сміття на сервер.
cmd-sendgarbage-help = Сервер відповість 'no u'

cmd-loadgrid-desc = Завантажує сітку з файлу в існуючу мапу.
cmd-loadgrid-help = використання: loadgrid <MapID> <Шлях> [x y] [rotation] [storeUids]

cmd-loc-desc = Виводить абсолютне місцезнаходження сутності гравця в консоль.
cmd-loc-help = використання: loc

cmd-tpgrid-desc = Телепортує сітку на нове місце.
cmd-tpgrid-help = використання: tpgrid <gridId> <X> <Y> [<MapId>]

cmd-rmgrid-desc = Видаляє сітку з мапи. Ви не можете видалити сітку за замовчуванням.
cmd-rmgrid-help = використання: rmgrid <gridId>

cmd-mapinit-desc = Ініціалізує мапу.
cmd-mapinit-help = використання: mapinit <mapID>

cmd-lsmap-desc = Перелічує мапи.
cmd-lsmap-help = використання: lsmap

cmd-lsgrid-desc = Перелічує сітки.
cmd-lsgrid-help = використання: lsgrid

cmd-addmap-desc = Додає нову порожню мапу до раунду. Якщо MapID вже існує, ця команда нічого не робить.
cmd-addmap-help = використання: addmap <mapID> [ініціалізувати]

cmd-rmmap-desc = Видаляє мапу зі світу. Ви не можете видалити карту з ідентифікатором nullspace.
cmd-rmmap-help = використання: rmmap <mapId>

cmd-savegrid-desc = Серіалізує сітку на диск.
cmd-savegrid-help = використання: savegrid <gridID> <Шлях>

cmd-testbed-desc = Завантажує фізичний тестовий стенд на вказаній мапі.
cmd-testbed-help = використання: testbed <mapid> <тест>


## Команда 'flushcookies'
# Примітка: команда flushcookies походить з Robust.Client.WebView, її немає в основному коді рушія.

cmd-flushcookies-desc = Очистити сховище файлів cookie CEF на диску
cmd-flushcookies-help = Це забезпечує належне збереження файлів cookie на диску у випадку нештатного завершення роботи.
    Зверніть увагу, що операція виконується асинхронно.

## Команда 'addcomp'
cmd-addcomp-desc = Додає компонент до сутності.
cmd-addcomp-help = використання: addcomp <uid> <componentName>
cmd-addcompc-desc = Додає компонент до сутності на клієнті.
cmd-addcompc-help = використання: addcompc <uid> <componentName>.

## Команда 'rmcomp'
cmd-rmcomp-desc = Видаляє компонент з сутності.
cmd-rmcomp-help = використання: rmcomp <uid> <componentName>
cmd-rmcompc-desc = Видаляє компонент з сутності на клієнті.
cmd-rmcompc-help = використання: rmcompc <uid> <componentName>.

## Команда 'addview'
cmd-addview-desc = Дозволяє підписатися на перегляд сутності для налагодження.
cmd-addview-help = використання: addview <entityUid>
cmd-addviewc-desc = Дозволяє підписатися на перегляд сутності для налагодження на клієнті.
cmd-addviewc-help = використання: addview <entityUid>.

## Команда 'removeview'
cmd-removeview-desc = Дозволяє скасувати підписку на перегляд сутності для налагодження.
cmd-removeview-help = використання: removeview <entityUid>.

## Команда 'loglevel'
cmd-loglevel-desc = Змінює рівень журналу для вказаної лісопилки.
cmd-loglevel-help = Використання: loglevel <sawmill> <level>.
      Лісопилка: мітка, що додається до повідомлень журналу. Це те, для чого ви встановлюєте рівень.
      Рівень: рівень журналу. Має відповідати одному з значень перерахування LogLevel.

cmd-testlog-desc = Пише тестовий запис у журнал для лісопилки.
cmd-testlog-help = Використання: testlog <sawmill> <level> <повідомлення>.
    Лісопилка: мітка, що додається до повідомлень журналу.
    Рівень: рівень журналу. Має відповідати одному з значень перерахування LogLevel.
    Повідомлення: повідомлення, яке буде записано до журналу. Використовуйте подвійні лапки, якщо хочете використати пробіли.

## Команда 'showvelocities'
cmd-showvelocities-desc = Відображає кутову та лінійну швидкість.
cmd-showvelocities-help = Використання: showvelocities.

## Команда 'setinputcontext'
cmd-setinputcontext-desc = Встановлює активний контекст введення.
cmd-setinputcontext-help = Використання: setinputcontext <контекст>.

## Команда 'forall'
cmd-forall-desc = Виконує команду для всіх сутностей із заданим компонентом.
cmd-forall-help = Використання: forall <bql-запит> do <команда...>.

## Команда 'delete'
cmd-delete-desc = Видаляє сутність із вказаним ідентифікатором.
cmd-delete-help = використання: delete <UID сутності>.

# Системні команди

cmd-showtime-desc = Показує час сервера.
cmd-showtime-help = використання: showtime

cmd-restart-desc = Граціозно перезапускає сервер (не тільки раунд).
cmd-restart-help = використання: restart

cmd-shutdown-desc = Граціозно вимикає сервер.
cmd-shutdown-help = використання: shutdown

cmd-saveconfig-desc = Зберігає конфігурацію сервера у файл.
cmd-saveconfig-help = використання: saveconfig

cmd-netaudit-desc = Виводить інформацію про безпеку NetMsg.
cmd-netaudit-help = використання: netaudit.

# Команди гравця

cmd-tp-desc = Телепортує гравця до будь-якого місця в раунді.
cmd-tp-help = використання: tp <x> <y> [<mapID>]

cmd-tpto-desc = Телепортує поточного гравця або вказаних гравців/сутностей до місцезнаходження першого гравця/сутності.
cmd-tpto-help = використання: tpto <ім'я користувача | uid> [ім'я користувача | uid]...
cmd-tpto-destination-hint = місце призначення (uid або ім'я користувача)
cmd-tpto-victim-hint = сутність для телепортації (uid або ім'я користувача)
cmd-tpto-parse-error = Неможливо визначити сутність або гравця: {$str}

cmd-listplayers-desc = Перелічує всіх гравців, які зараз підключені.
cmd-listplayers-help = використання: listplayers

cmd-kick-desc = Виганяє підключеного гравця з сервера, від'єднуючи його.
cmd-kick-help = використання: kick <індекс гравця> [<причина>].

# Команда обертання

cmd-spin-desc = Спричиняє обертання сутності. За замовчуванням сутність є батьком приєднаного гравця.
cmd-spin-help = використання: spin <швидкість обертання> [drag] [entityUid].

# Команда локалізації

cmd-rldloc-desc = Перезавантажує локалізацію (клієнт і сервер).
cmd-rldloc-help = Використання: rldloc.

# Команди налагодження сутностей

cmd-spawn-desc = Створює сутність із вказаним типом.
cmd-spawn-help = використання: spawn <прототип> або spawn <прототип> <ідентифікатор сутності> або spawn <прототип> <x> <y>

cmd-cspawn-desc = Створює клієнтську сутність із вказаним типом біля ваших ніг.
cmd-cspawn-help = використання: cspawn <тип сутності>

cmd-scale-desc = Збільшує або зменшує розмір сутності.
cmd-scale-help = використання: scale <entityUid> <float>

cmd-dumpentities-desc = Вивантажує список сутностей.
cmd-dumpentities-help = Використання: dumpentities.

cmd-getcomponentregistration-desc = Отримує інформацію про реєстрацію компонента.
cmd-getcomponentregistration-help = Використання: getcomponentregistration <назва компонента>

cmd-showrays-desc = Вмикає налагоджувальний малюнок фізичних променів. Має бути задано ціле число для тривалості променя.
cmd-showrays-help = Використання: showrays <raylifetime>

cmd-disconnect-desc = Негайно відключає від сервера та повертає до головного меню.
cmd-disconnect-help = Використання: disconnect

cmd-entfo-desc = Відображає розширену діагностику для сутності.
cmd-entfo-help = Використання: entfo <entityuid>.
    UID сутності можна префіксувати 'c', щоб перетворити його на UID клієнтської сутності.

cmd-fuck-desc = Викликає виключення
cmd-fuck-help = Викликає виключення

cmd-showpos-desc = Вмикає налагоджувальний малюнок над усіма позиціями сутностей у грі.
cmd-showpos-help = Використання: showpos

cmd-sggcell-desc = Перелічує сутності у клітинці сітки прив'язки.
cmd-sggcell-help = Використання: sggcell <gridID> <vector2i>.
    Параметр vector2i має вигляд x<int>,y<int>.

cmd-overrideplayername-desc = Змінює ім'я, яке використовується при спробі підключення до сервера.
cmd-overrideplayername-help = Використання: overrideplayername <ім'я>

cmd-showanchored-desc = Показує прив'язані сутності на певній плитці
cmd-showanchored-help = Використання: showanchored

cmd-dmetamem-desc = Відображає межі фрагментів для цілей рендерингу.
cmd-dmetamem-help = Використання: showchunkbb <тип>

cmd-launchauth-desc = Завантажує токени автентифікації з даних панелі запуску для полегшення тестування серверів у реальному часі.
cmd-launchauth-help = Використання: launchauth <назва акаунта>

cmd-lightbb-desc = Перемикає відображення обмежувальних рамок освітлення.
cmd-lightbb-help = Використання: lightbb

cmd-monitorinfo-desc = Виводить інформацію про монітор
cmd-monitorinfo-help = Використання: monitorinfo <id>

cmd-setmonitor-desc = Встановлює монітор
cmd-setmonitor-help = Використання: setmonitor <id>

cmd-physics-desc = Показує налагоджувальне накладання фізики. Аргумент визначає тип накладання.
cmd-physics-help = Використання: physics <aabbs / com / contactnormals / contactpoints / distance / joints / shapeinfo / shapes>

cmd-hardquit-desc = Миттєво завершує роботу ігрового клієнта.
cmd-hardquit-help = Миттєво завершує роботу ігрового клієнта, не залишаючи жодних слідів і не повідомляючи серверу.

cmd-quit-desc = Граціозно завершує роботу ігрового клієнта.
cmd-quit-help = Коректно завершує роботу ігрового клієнта, повідомляючи про це підключений сервер.

cmd-csi-desc = Відкриває інтерактивну консоль C#.
cmd-csi-help = Використання: csi

cmd-scsi-desc = Відкриває інтерактивну консоль C# на сервері.
cmd-scsi-help = Використання: scsi

cmd-watch-desc = Відкриває вікно спостереження за змінними.
cmd-watch-help = Використання: watch

cmd-showspritebb-desc = Перемикає відображення обмежувальних рамок спрайтів
cmd-showspritebb-help = Використання: showspritebb

cmd-togglelookup-desc = Перемикає відображення обмежувальних рамок пошуку сутностей.
cmd-togglelookup-help = Використання: togglelookup

cmd-net_entityreport-desc = Перемикає панель звітів про мережеві сутності.
cmd-net_entityreport-help = Використання: net_entityreport

cmd-net_refresh-desc = Запитує повний стан сервера.
cmd-net_refresh-help = Використання: net_refresh

cmd-net_graph-desc = Перемикає відображення панелі статистики мережі.
cmd-net_graph-help = Використання: net_graph

cmd-net_watchent-desc = Виводить у консоль усі мережеві оновлення для EntityId.
cmd-net_watchent-help = Використання: net_watchent <0|EntityUid>

cmd-net_draw_interp-desc = Перемикає відображення налагоджувального малюнка мережевої інтерполяції.
cmd-net_draw_interp-help = Використання: net_draw_interp <0|EntityUid>

cmd-vram-desc = Відображає статистику використання відеопам'яті грою.
cmd-vram-help = Використання: vram

cmd-showislands-desc = Показує поточні фізичні тіла, задіяні на кожному фізичному острові.
cmd-showislands-help = Використання: showislands

cmd-showgridnodes-desc = Показує вузли для розділення сітки.
cmd-showgridnodes-help = Використання: showgridnodes

cmd-profsnap-desc = Створює знімок для профілювання.
cmd-profsnap-help = Використання: profsnap

cmd-devwindow-desc = Відкриває файл
cmd-devwindow-help = Використання: testopenfile

cmd-scene-desc = Негайно змінює сцену/стан інтерфейсу.
cmd-scene-help = Використання: scene <ім'я класу>

cmd-szr_stats-desc = Виводить статистику серіалізатора.
cmd-szr_stats-help = Використання: szr_stats

cmd-hwid-desc = Повертає поточний HWID (Hardware ID).
cmd-hwid-help = Використання: hwid

cmd-vvread-desc = Отримує значення за вказаним шляхом за допомогою VV (View Variables)
cmd-vvread-help = Використання: vvread <шлях>.

cmd-vvwrite-desc = Змінює значення за вказаним шляхом за допомогою VV (View Variables).
cmd-vvwrite-help = Використання: vvwrite <шлях>

cmd-vv-desc = Відкриває вікно перегляду змінних (VV).
cmd-vv-help = Використання: vv <шлях | ідентифікатор сутності | guihover>

cmd-vvinvoke-desc = Викликає метод за вказаним шляхом з аргументами за допомогою VV.
cmd-vvinvoke-help = Використання: vvinvoke <шлях> [аргументи...]

cmd-dump_dependency_injectors-desc = Вивантажує кеш інжектора залежностей IoCManager.
cmd-dump_dependency_injectors-help = Використання: dump_dependency_injectors
cmd-dump_dependency_injectors-total-count = Загальна кількість: {$total}

cmd-dump_netserializer_type_map-desc = Вивантажує карту типів і хеш серіалізатора NetSerializer.
cmd-dump_netserializer_type_map-help = Використання: dump_netserializer_type_map

cmd-hub_advertise_now-desc = Негайно рекламує сервер у майстер-хаб
cmd-hub_advertise_now-help = Використання: hub_advertise_now

cmd-echo-desc = Повертає аргументи назад у консоль
cmd-echo-help = Використання: echo "<повідомлення>".

## Команда 'vfs_ls'
cmd-vfs_ls-desc = Перелічує вміст каталогу у VFS.
cmd-vfs_ls-help = Використання: vfs_list <шлях>.
    Приклад:
    vfs_list /Assemblies

cmd-vfs_ls-err-args = Потрібен рівно один аргумент.
cmd-vfs_ls-hint-path = <шлях>

cmd-reloadtiletextures-desc = Перезавантажує атлас текстур плиток для гарячого перезавантаження спрайтів
cmd-reloadtiletextures-help = Використання: reloadtiletextures

cmd-audio_length-desc = Показує тривалість аудіофайлу
cmd-audio_length-help = Використання: audio_length {cmd-audio_length-arg-file-name}
cmd-audio_length-arg-file-name = <ім'я файлу>.

## PVS
cmd-pvs-override-info-desc = Виводить інформацію про перевизначення PVS, пов'язані з сутністю.
cmd-pvs-override-info-empty = Сутність {$nuid} не має перевизначень PVS.
cmd-pvs-override-info-global = Сутність {$nuid} має глобальне перевизначення.
cmd-pvs-override-info-clients = Сутність {$nuid} має перевизначення сеансу для {$clients}.
