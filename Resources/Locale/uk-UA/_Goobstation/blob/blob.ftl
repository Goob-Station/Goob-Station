# SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
# SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
# SPDX-FileCopyrightText: 2024 lanse12 <cloudability.ez@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 GitHubUser53123 <110841413+GitHubUser53123@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
# SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

ent-SpawnPointGhostBlob = Точка появи Клякси
    .suffix = DEBUG, Точка появи примар
    .desc = { ent-MarkerBase.desc }
ent-MobBlobPod = Спороносець Клякси
    .desc = Звичайний боєць Клякси.
ent-MobBlobBlobbernaut = Кляксонавт
    .desc = Елітний боєць Клякси.
ent-BaseBlob = основа клякси
    .desc = { "" }
ent-NormalBlobTile = Звичайна Плитка Клякси
    .desc = Звичайна частина Клякси, необхідна для створення більш просунутих плиток.
ent-CoreBlobTile = Ядро Клякси
    .desc = Найважливіший орган Клякси. Знищивши ядро, інфекція припиниться.
ent-FactoryBlobTile = Фабрика Клякси
    .desc = З часом створює Десант Клякси та Кляксонавтів.
ent-ResourceBlobTile = Ресурсна Клякса
    .desc = Виробляє ресурси для Клякси.
ent-NodeBlobTile = Вузол Клякси
    .desc = Міні-версія ядра, що дозволяє розміщувати спеціальні плитки Клякси навколо себе.
ent-StrongBlobTile = Міцна Плитка Клякси
    .desc = Посилена версія звичайної плитки. Вона не пропускає повітря та захищає від фізичних ушкоджень.
ent-ReflectiveBlobTile = Відбиваюча Плитка Клякси
    .desc = Відбиває лазери, але гірше захищає від фізичних ушкоджень.
    .desc = { "" }
objective-issuer-blob = Клякса


ghost-role-information-blobbernaut-name = Кляксонавт
ghost-role-information-blobbernaut-description = Ви - Кляксонавт. Ви повинні захищати ядро Клякси. Використовуйте + або +e в чаті, щоб спілкуватися у колективному розумі Клякси.

ghost-role-information-blob-name = Клякса
ghost-role-information-blob-description = Ви - Інфекція Клякси. Поглиньте станцію.

roles-antag-blob-name = Клякса
roles-antag-blob-objective = Досягти критичної маси.

guide-entry-blob = Клякса

# Спливаючі вікна
blob-target-normal-blob-invalid = Неправильний тип клякси, виберіть звичайну кляксу.
blob-target-factory-blob-invalid = Неправильний тип клякси, виберіть фабрику клякси.
blob-target-node-blob-invalid = Неправильний тип клякси, виберіть вузол клякси.
blob-target-close-to-resource = Занадто близько до іншої ресурсної клякси.
blob-target-nearby-not-node = Поблизу немає вузла або ресурсної клякси.
blob-target-close-to-node = Занадто близько до іншого вузла.
blob-target-already-produce-blobbernaut = Ця фабрика вже виробила кляксонавта.
blob-cant-split = Ви не можете розділити ядро клякси.
blob-not-have-nodes = У вас немає вузлів.
blob-not-enough-resources = Недостатньо ресурсів.
blob-help = Тільки Бог може вам допомогти.
blob-swap-chem = У розробці.
blob-mob-attack-blob = Ви не можете атакувати кляксу.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = Ви вмираєте, перебуваючи не на плитках клякси.
carrier-blob-alert = У вас залишилося { $second } секунд до трансформації.

blob-mob-zombify-second-start = { $pod } починає перетворювати вас на зомбі.
blob-mob-zombify-third-start = { $pod } починає перетворювати { $target } на зомбі.

blob-mob-zombify-second-end = { $pod } перетворює вас на зомбі.
blob-mob-zombify-third-end = { $pod } перетворює { $target } на зомбі.

blobberaut-factory-destroy = фабрика знищена
blob-target-already-connected = вже підключено


# Інтерфейс
blob-chem-swap-ui-window-name = Змінити хімікати
blob-chem-reactivespines-info = Реактивні Шипи
                                Наносить 25 фізичної шкоди.
blob-chem-blazingoil-info = Палаюча Олія
                            Наносить 15 шкоди від опіків і підпалює цілі.
                            Робить вас вразливими до води.
blob-chem-regenerativemateria-info = Регенеративна Матерія
                                    Наносить 6 фізичної шкоди та 15 шкоди від токсинів.
                                    Ядро клякси відновлює здоров'я вдесятеро швидше та генерує 1 додатковий ресурс.
blob-chem-explosivelattice-info = Вибухова Решітка
                                    Наносить 5 шкоди від опіків і вибухає ціль, наносячи 10 фізичної шкоди.
                                    Спори вибухають після смерті.
                                    Ви стаєте імунними до вибухів.
                                    Ви отримуєте на 50% більше шкоди від опіків та електричного шоку.
blob-chem-electromagneticweb-info = Електромагнітна Мережа
                                    Наносить 20 шкоди від опіків, 20% шанс викликати ЕМІ-імпульс при атаці.
                                    Плитки клякси викликають ЕМІ-імпульс при знищенні.
                                    Ви отримуєте на 25% більше фізичної та термічної шкоди.

blob-alert-out-off-station = Кляксу було видалено, оскільки її знайшли за межами станції!

# Оголошення
blob-alert-recall-shuttle = Евакуаційний шатл неможливо відправити, поки на станції присутня біологічна небезпека 5 рівня.
blob-alert-detect = Підтверджено спалах біологічної небезпеки 5-го рівня на борту станції. Весь персонал повинен стримувати спалах.
blob-alert-critical = Рівень біологічної небезпеки критичний, коди ядерної автентифікації надіслані на станцію. Центральне командування наказує всьому персоналу, що залишився, активувати механізм самознищення.
blob-alert-critical-NoNukeCode = Рівень біологічної небезпеки критичний. Центральне Командування наказує персоналу, що залишився, знайти укриття та чекати на порятунок.

# Дії
blob-create-factory-action-name = Розмістити Фабрику Клякси (80)
blob-create-factory-action-desc = Перетворює обрану звичайну кляксу на фабрику клякси, яка вироблятиме до 3 спор та кляксонавта, якщо розміщена поруч з ядром або вузлом.
blob-create-resource-action-name = Розмістити Ресурсну Кляксу (60)
blob-create-resource-action-desc = Перетворює обрану звичайну кляксу на ресурсну кляксу, яка генеруватиме ресурси, якщо розміщена поруч з ядром або вузлом.
blob-create-node-action-name = Розмістити Вузол Клякси (50)
blob-create-node-action-desc = Перетворює обрану звичайну кляксу на вузол клякси.
                                Вузол клякси активуватиме ефекти фабрик та ресурсних клякс, лікуватиме інші клякси та повільно розширюватиметься, руйнуючи стіни та створюючи звичайні клякси.
blob-produce-blobbernaut-action-name = Виробити Кляксонавта (60)
blob-produce-blobbernaut-action-desc = Створює кляксонавта на обраній фабриці. Кожна фабрика може зробити це лише один раз. Кляксонавт отримуватиме шкоду за межами плиток клякси та лікуватиметься, перебуваючи поруч з вузлами.
blob-split-core-action-name = Розділити Ядро (400)
blob-split-core-action-desc = Ви можете зробити це лише один раз. Перетворює обраний вузол на незалежне ядро, яке діятиме самостійно.
blob-swap-core-action-name = Перемістити Ядро (200)
blob-swap-core-action-desc = Міняє місцями ваше ядро та обраний вузол.
blob-teleport-to-core-action-name = Стрибнути до Ядра (0)
blob-teleport-to-core-action-desc = Телепортує вас до вашого Ядра Клякси.
blob-teleport-to-node-action-name = Стрибнути до Вузла (0)
blob-teleport-to-node-action-desc = Телепортує вас до випадкового вузла клякси.
blob-help-action-name = Допомога
blob-help-action-desc = Отримати базову інформацію про гру за кляксу.
blob-swap-chem-action-name = Змінити хімікати (70)
blob-swap-chem-action-desc = Дозволяє змінити ваш поточний хімікат.
blob-carrier-transform-to-blob-action-name = Перетворитися на кляксу
blob-carrier-transform-to-blob-action-desc = Миттєво знищує ваше тіло та створює ядро клякси. Переконайтеся, що ви стоїте на плитці підлоги, інакше ви просто зникнете.
blob-downgrade-action-name = понизити кляксу(0)
blob-downgrade-action-desc = Перетворює обрану плитку назад у звичайну кляксу, щоб встановити інші типи плиток.

# Роль привида
blob-carrier-role-name = Носій Клякси
blob-carrier-role-desc = Істота, заражена кляксою.
blob-carrier-role-rules = Ви антагоніст. У вас є 10 хвилин, перш ніж ви перетворитеся на кляксу.
                        Використовуйте цей час, щоб знайти безпечне місце на станції. Майте на увазі, що ви будете дуже слабкими одразу після трансформації.
blob-carrier-role-greeting = Ви носій Клякси. Знайдіть відокремлене місце на станції та перетворіться на Кляксу. Поглиньте станцію, а її мешканців перетворіть на своїх слуг. Ми всі - Клякси.

# Дієслова
blob-pod-verb-zombify = Зомбіфікувати
blob-verb-upgrade-to-strong = Покращити до Міцної Клякси
blob-verb-upgrade-to-reflective = Покращити до Відбиваючої Клякси
blob-verb-remove-blob-tile = Видалити Кляксу

# Сповіщення
blob-resource-alert-name = Ресурси Ядра
blob-resource-alert-desc = Ваші ресурси, вироблені ядром та ресурсними кляксами. Використовуйте їх для розширення та створення спеціальних клякс.
blob-health-alert-name = Здоров'я Ядра
blob-health-alert-desc = Здоров'я вашого ядра. Ви помрете, якщо воно досягне нуля.

# Привітання
blob-role-greeting = Ви клякса - паразитична космічна істота, здатна знищувати цілі станції.
        Ваша мета - вижити і вирости якомога більшими.
        Ви майже невразливі до фізичних пошкоджень, але спека все ще може завдати вам шкоди.
        Використовуйте Alt+ЛКМ, щоб покращити звичайні плитки клякси до міцних, а міцні - до відбиваючих.
        Обов'язково розміщуйте ресурсні клякси для генерації ресурсів.
        Майте на увазі, що ресурсні клякси та фабрики працюватимуть лише поруч з вузлами клякси або ядрами.
        Ви можете використовувати + або +e в чаті, щоб використовувати Розум Клякси для спілкування зі своїми поплічниками.
blob-zombie-greeting = Ви були заражені та вирощені спорою клякси. Тепер ви повинні допомогти кляксі захопити станцію. Використовуйте +e в чаті, щоб спілкуватися в Розумі Клякси.

# Кінець раунду
blob-round-end-result = { $blobCount ->
        [one] Була одна інфекція клякси.
        *[other] Було {$blobCount} клякс.
    }

blob-user-was-a-blob = [color=gray]{$user}[/color] був кляксою.
blob-user-was-a-blob-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) був кляксою.
blob-was-a-blob-named = [color=White]{$name}[/color] був кляксою.

preset-blob-objective-issuer-blob = [color=#33cc00]Клякса[/color]

blob-user-was-a-blob-with-objectives = [color=gray]{$user}[/color] був кляксою, яка мала наступні цілі:
blob-user-was-a-blob-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) був кляксою, яка мала наступні цілі:
blob-was-a-blob-with-objectives-named = [color=White]{$name}[/color] був кляксою, яка мала наступні цілі:

# Цілі
objective-condition-blob-capture-title = Захопити станцію
objective-condition-blob-capture-description = Ваша єдина мета - захопити всю станцію. Вам потрібно мати щонайменше {$count} плиток клякси.
objective-condition-success = { $condition } | [color={ $markupColor }]Успіх![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Провал![/color] ({ $progress }%)

# Команди адміністратора

admin-verb-make-blob = Зробити ціль носієм клякси.
admin-verb-text-make-blob = Зробити носієм Клякси

language-Blob-name = Клякса
chat-language-Blob-name = Клякса
language-Blob-description = Бліб боб! Блоб блоб!