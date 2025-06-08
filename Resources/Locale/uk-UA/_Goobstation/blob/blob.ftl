ent-SpawnPointGhostBlob = Спавнер блоба
    .suffix = DEBUG, Спавнер ролі примари
    .desc = { ent-MarkerBase.desc }
ent-MobBlobPod = Blob Drop
    .desc = Звичайний боєць блоба.
ent-MobBlobBlobbernaut = Blobbernaut
    .desc = Елітний боєць блоба.
ent-BaseBlob = базовий блоб
    .desc = { "" }
ent-NormalBlobTile = Звичайний тайл блоба
    .desc = Звичайна частина блоба, необхідна для побудови більш складних тайлів.
ent-CoreBlobTile = Ядро блоба
    .desc = Найважливіший орган блоба. Якщо знищити ядро, інфекція припиниться.
ent-FactoryBlobTile = Фабрика блоба
    .desc = З часом створює Blob Drops та Blobbernauts.
ent-ResourceBlobTile = Ресурсний блоб
    .desc = Створює ресурси для блоба.
ent-NodeBlobTile = Вузловий блоб
    .desc = Міні-версія ядра, що дозволяє розміщувати спеціальні тайли блоба навколо себе.
ent-StrongBlobTile = Сильний тайл блоба
    .desc = Посилена версія звичайного тайла. Не пропускає повітря і захищає від грубих пошкоджень.
ent-ReflectiveBlobTile = Відбивний тайл блоба
    .desc = Відбиває лазери, але не так добре захищає від грубих пошкоджень.
    .desc = { "" }
objective-issuer-blob = Блоб


ghost-role-information-blobbernaut-name = Блоббернаут
ghost-role-information-blobbernaut-description = Ти — Блоббернаут. Ти повинен захищати ядро блоба.

ghost-role-information-blob-name = Блоб
ghost-role-information-blob-description = Ти — Блоб-інфекція. Поглинай станцію.

roles-antag-blob-name = Блоб
roles-antag-blob-objective = Досягти критичної маси.

guide-entry-blob = Блоб

# Popups
blob-target-normal-blob-invalid = Неправильний тип блоба, оберіть звичайний блоб.
blob-target-factory-blob-invalid = Неправильний тип блоба, оберіть фабричний блоб.
blob-target-node-blob-invalid = Неправильний тип блоба, оберіть вузловий блоб.
blob-target-close-to-resource = Занадто близько до іншого ресурсного блоба.
blob-target-nearby-not-node = Поблизу немає вузла або ресурсного блоба.
blob-target-close-to-node = Занадто близько до іншого вузла.
blob-target-already-produce-blobbernaut = На цій фабриці вже було створено блоббернаута.
blob-cant-split = Ви не можете розділити ядро блоба.
blob-not-have-nodes = У вас немає вузлів.
blob-not-enough-resources = Недостатньо ресурсів.
blob-help = Тільки Бог може тобі допомогти.
blob-swap-chem = У розробці.
blob-mob-attack-blob = Ви не можете атакувати блоб.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = Ви помираєте, перебуваючи не на тайлах блоба.
carrier-blob-alert = У вас залишилося { $second } секунд до трансформації.

blob-mob-zombify-second-start = { $pod } починає перетворювати вас на зомбі.
blob-mob-zombify-third-start = { $pod } починає перетворювати { $target } на зомбі.

blob-mob-zombify-second-end = { $pod } перетворює вас на зомбі.
blob-mob-zombify-third-end = { $pod } перетворює { $target } на зомбі.

blobberaut-factory-destroy = знищення фабрики
blob-target-already-connected = уже підключено


# UI
blob-chem-swap-ui-window-name = Замінити хімічні речовини
blob-chem-reactivespines-info = Реактивні шипи
                                Завдає 25 одиниць грубої шкоди.
blob-chem-blazingoil-info = Палаюча олія
                            Завдає 15 одиниць опікової шкоди й підпалює цілі.
                            Робить вас вразливими до води.
blob-chem-regenerativemateria-info = Регенеративна матерія
                                    Завдає 6 одиниць грубої шкоди й 15 одиниць токсичної шкоди.
                                    Ядро блоба відновлює здоров’я у 10 разів швидше за норму і генерує 1 додатковий ресурс.
blob-chem-explosivelattice-info = Вибухова решітка
                                    Завдає 5 одиниць опікової шкоди та підриває ціль, завдаючи 10 одиниць грубої шкоди.
                                    Спори вибухають після смерті.
                                    Ви стаєте несприйнятливими до вибухів.
                                    Ви отримуєте на 50% більше шкоди від опіків та ураження електрикою.
blob-chem-electromagneticweb-info = Електромагнітна павутина
                                    Завдає 20 одиниць опікової шкоди, 20% шанс викликати імпульс ЕМП під час атаки.
                                    Тайли блоба викликають імпульс ЕМП під час знищення.
                                    Ви отримуєте на 25% більше грубої та теплової шкоди.

blob-alert-out-off-station = Блоб було видалено, оскільки він перебував за межами станції!

# Announcment
blob-alert-recall-shuttle = Аварійний шатл не може бути відправлений, поки на станції є біонебезпека 5-го рівня.
blob-alert-detect = Підтверджено спалах біологічної небезпеки 5-го рівня на борту станції. Весь персонал мусить зупинити поширення.
blob-alert-critical = Рівень біонебезпеки критичний, на станцію надіслано коди ядерної автентифікації. Центральне командування наказує будь-кому з уцілілих активувати механізм самознищення.
blob-alert-critical-NoNukeCode = Рівень біонебезпеки критичний. Центральне командування наказує тим, хто вижив, шукати укриття та чекати на рятувальну команду.

# Actions
blob-create-factory-action-name = Розмістити фабричний блоб (80)
blob-create-factory-action-desc = Перетворює вибраний звичайний блоб на фабричний блоб, який створює до 3 спор і блоббернаута, якщо розміщений поруч із ядром або вузлом.
blob-create-resource-action-name = Розмістити ресурсний блоб (60)
blob-create-resource-action-desc = Перетворює вибраний звичайний блоб на ресурсний, який генерує ресурси, якщо розмістити його поруч із ядром або вузлом.
blob-create-node-action-name = Розмістити вузловий блоб (50)
blob-create-node-action-desc = Перетворює вибраний звичайний блоб на вузловий.
                                Вузловий блоб активує ефекти фабричних і ресурсних блобів, зцілює інші блоби й повільно розширюється, руйнуючи стіни й створюючи звичайні блоби.
blob-produce-blobbernaut-action-name = Створити блоббернаута (60)
blob-produce-blobbernaut-action-desc = Створює блоббернаута на вибраній фабриці. Кожна фабрика може зробити це лише раз. Блоббернаут отримує пошкодження за межами тайлів блоба й відновлюється, коли поруч є вузли.
blob-split-core-action-name = Розділити ядро (400)
blob-split-core-action-desc = Ви можете зробити це лише один раз. Перетворює вибраний вузол на незалежне ядро, яке діятиме самостійно.
blob-swap-core-action-name = Перемістити ядро (200)
blob-swap-core-action-desc = Міняє місцями розташування вашого ядра та вибраного вузла.
blob-teleport-to-core-action-name = Переміститися до ядра (0)
blob-teleport-to-core-action-desc = Телепортує вас до вашого ядра блоба.
blob-teleport-to-node-action-name = Переміститися до вузла (0)
blob-teleport-to-node-action-desc = Телепортує вас до випадкового вузлового блоба.
blob-help-action-name = Допомога
blob-help-action-desc = Отримайте базову інформацію про гру за блоба.
blob-swap-chem-action-name = Замінити хімічні речовини (70)
blob-swap-chem-action-desc = Дозволяє змінити вашу поточну хімічну речовину.
blob-carrier-transform-to-blob-action-name = Перетворитися на блоб
blob-carrier-transform-to-blob-action-desc = Миттєво знищує ваше тіло та створює ядро блоба. Переконайтеся, що ви стоїте на плитці підлоги, інакше ви просто зникнете.
blob-downgrade-action-name = знизити рівень блоба (0)
blob-downgrade-action-desc = Перетворює вибраний тайл назад у звичайний блоб, щоб установити інші типи кліток.

# Ghost role
blob-carrier-role-name = Носій блоба
blob-carrier-role-desc = Істота, заражена блобом.
blob-carrier-role-rules = Ви — антагоніст. У вас є 4 хвилини, перш ніж ви перетворитеся на блоб.
                        Використайте цей час, щоб знайти безпечне місце на станції. Пам’ятайте, що одразу після трансформації ви будете дуже слабкими.
blob-carrier-role-greeting = Ви — носій Блоба. Знайдіть відокремлене місце на станції й перетворіться на Блоб. Зробіть станцію масою, а її мешканців — своїми слугами. Ми всі — Блоби.

# Verbs
blob-pod-verb-zombify = Зомбіфікувати
blob-verb-upgrade-to-strong = Оновити до Strong Blob
blob-verb-upgrade-to-reflective = Оновити до Reflective Blob
blob-verb-remove-blob-tile = Видалити блоб

# Alerts
blob-resource-alert-name = Ресурси ядра
blob-resource-alert-desc = Ваші ресурси, які виробляють ядро та ресурсні блоби. Використовуйте їх, щоб розширитися та створювати спеціальні блоби.
blob-health-alert-name = Здоров’я ядра
blob-health-alert-desc = Здоров’я вашого ядра. Ви загинете, якщо воно досягне нуля.

# Greeting
blob-role-greeting = Ви — блоб — паразитична космічна істота, здатна знищувати цілі станції.
        Ваша мета — вижити й вирости якомога більшою.
    	Ви майже невразливі до фізичних ушкоджень, але тепло все ж може завдавати вам шкоди.
        Використовуйте Alt+E, щоб оновити звичайні тайли блоба до сильних, а сильні — до відбивних.
    	Обов’язково розміщуйте ресурсні блоби, щоб генерувати ресурси.
        Пам’ятайте, що ресурсні блоби й фабрики працюють лише тоді, коли поруч є вузлові блоби або ядро.
blob-zombie-greeting = Вас заразили й виростили спори блоба. Тепер ви повинні допомогти блобу захопити станцію.

# End round
blob-round-end-result = { $blobCount ->
        [one] Був виявлений один блоб.
        *[other] Було виявлено {$blobCount} блобів.
    }

blob-user-was-a-blob = [color=gray]{$user}[/color] був блобом.
blob-user-was-a-blob-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) був блобом.
blob-was-a-blob-named = [color=White]{$name}[/color] був блобом.

preset-blob-objective-issuer-blob = [color=#33cc00]Блоб[/color]

blob-user-was-a-blob-with-objectives = [color=gray]{$user}[/color] був блобом, що мав наступні цілі:
blob-user-was-a-blob-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) був блобом, що мав наступні цілі:
blob-was-a-blob-with-objectives-named = [color=White]{$name}[/color] був блобом, що мав наступні цілі:

# Objectivies
objective-condition-blob-capture-title = Захопити станцію
objective-condition-blob-capture-description = Ваша єдина мета — захопити всю станцію. Вам потрібно мати принаймні {$count} тайлів блоба.
objective-condition-success = { $condition } | [color={ $markupColor }]Успіх![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Невдача![/color] ({ $progress }%)

blob-title = Блоб-інфекція
blob-description = Блоб-інфекція — це паразитична космічна істота, здатна знищувати цілі станції.
admin-verb-text-make-blob = Зробити гравця блоб-інфекцією

blob-core-under-attack = Ваше ядро атаковано!
blob-create-storage-action-name = Створити Сховище Блоба (50)
blob-create-storage-action-desc = Перетворює обраний звичайний блоб на сховище, що збільшує максимальну кількість ресурсів, які може мати блоб.
blob-create-turret-action-name = Створити Турель Блоба (75)
blob-create-turret-action-desc = Перетворює обраний звичайний блоб на турель, що стріляє по ворогах малими спорами, споживаючи очки.
blob-objective-percentage = Він захопив [color=White]{ $progress }%[/color] до перемоги.
blob-end-victory = [color=Red]Блоб(и) успішно поглинув(ли) станцію![/color]
blob-end-fail = [color=Green]Блоб(и) не зміг(ли) поглинути станцію.[/color]
speak-vv-blob = Блоб
blob-end-fail-progress = Усі блоби захопили [color=Yellow]{ $progress }%[/color] до перемоги.
speak-vv-blob-core = Ядро блоба

# Мова
language-Blob-name = Блоб
chat-language-Blob-name = Блоб
language-Blob-description = Бліб боб! Блоб блоб!