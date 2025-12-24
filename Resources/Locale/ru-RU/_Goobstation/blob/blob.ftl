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

objective-issuer-blob = Блоб
ghost-role-information-blobbernaut-name = Блоббернаут
ghost-role-information-blobbernaut-description = Вы — блоббернаут. Вы должны защищать ядро блоба. Используйте + или +e в чате, чтобы говорить в Коллективный разум Блоба.
ghost-role-information-blob-name = Блоб
ghost-role-information-blob-description = Вы — инфекция Блоба. Поглощайте станцию.
roles-antag-blob-name = Блоб
roles-antag-blob-objective = Достигните критической массы.
guide-entry-blob = Блоб
# Popups
blob-target-normal-blob-invalid = Неверный тип блоба, выберите обычный блоб.
blob-target-factory-blob-invalid = Неверный тип блоба, выберите фабричный блоб.
blob-target-node-blob-invalid = Неверный тип блоба, выберите узловой блоб.
blob-target-close-to-resource = Слишком близко к другому ресурсному блобу.
blob-target-nearby-not-node = Поблизости нет узлового или ресурсного блоба.
blob-target-close-to-node = Слишком близко к другому узлу.
blob-target-already-produce-blobbernaut = Эта фабрика уже произвела блоббернаута.
blob-cant-split = Вы не можете разделить ядро блоба.
blob-not-have-nodes = У вас нет узлов.
blob-not-enough-resources = Недостаточно ресурсов.
blob-help = Только Бог может вам помочь.
blob-swap-chem = В разработке.
blob-mob-attack-blob = Вы не можете атаковать блоба.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = Вы умираете вне плиток блоба.
carrier-blob-alert = У вас осталось { $second } секунд до превращения.
blob-mob-zombify-second-start = { $pod } начинает превращать вас в подконтрольного.
blob-mob-zombify-third-start = { $pod } начинает превращать { $target } в подконтрольного.
blob-mob-zombify-second-end = { $pod } превратил вас в подконтрольного.
blob-mob-zombify-third-end = { $pod } превратил { $target } в подконтрольного.
blobberaut-factory-destroy = фабрика уничтожена
blob-target-already-connected = уже подключено
# UI
blob-chem-swap-ui-window-name = Обмен химикатами
blob-chem-reactivespines-info =
    Реактивные шипы
    Наносят 25 единиц урона ушибами.
blob-chem-blazingoil-info =
    Пылающее масло
    Наносит 15 урона от ожогов и поджигает цель.
    Делает уязвимым к воде.
blob-chem-regenerativemateria-info =
    Регенеративная материя
    Наносит 6 урона ушибами и 15 токсина.
    Ядро блоба восстанавливает здоровье в 10 раз быстрее и генерирует +1 ресурс.
blob-chem-explosivelattice-info =
    Взрывчатая решетка
    Наносит 5 урона от ожогов и взрывает цель, нанося 10 грубого урона.
    Споры взрываются при смерти.
    У вас иммунитет к взрывам.
    Получаете на 50% больше урона от ожогов и электричества.
blob-chem-electromagneticweb-info =
    Электромагнитная паутина
    Наносит 20 урона от ожогов, 20% шанс вызвать импульс ЭМИ при атаке.
    Плитки блоба вызывают ЭМИ при уничтожении.
    Получаете на 25% больше урона ушибами и ожога.
blob-alert-out-off-station = Блоб был удалён, так как находился вне станции!
# Announcment
blob-alert-recall-shuttle = Аварийный шаттл не может быть вызван, пока на станции присутствует биологическая угроза 5-го уровня.
blob-alert-detect = Подтверждена вспышка биологической угрозы 5-го уровня на станции. Все сотрудники обязаны сдерживать угрозу.
blob-alert-critical = Критический уровень биологической угрозы. На станцию отправлены коды ядерной аутентификации. Центральное командование приказывает всем оставшимся членам экипажа активировать механизм самоуничтожения.
blob-alert-critical-NoNukeCode = Критический уровень биологической угрозы. Центральное командование приказывает оставшимся членам экипажа укрыться и ожидать помощь.
# Actions
blob-teleport-to-node-action-name = Телепорт к узлу (0)
blob-teleport-to-node-action-desc = Телепортирует вас к случайному узловому блобу.
blob-help-action-name = Помощь
blob-help-action-desc = Получить базовую информацию об игре за блоба.
# Ghost role
blob-carrier-role-name = Носитель блоба
blob-carrier-role-desc = Существо, инфицированное блобом.
blob-carrier-role-rules =
    Вы — антагонист. У вас есть 10 минут до превращения в блоба.
    Используйте это время, чтобы найти безопасное место на станции. Учтите, что после превращения вы будете очень слабы.
blob-carrier-role-greeting = Вы — носитель блоба. Найдите укромное место на станции и превратитесь в блоба. Превратите станцию в массу, а её обитателей в своих слуг. Мы все — блобы.
# Verbs
blob-pod-verb-zombify = Подчинить
blob-verb-upgrade-to-strong = Улучшить до усиленного блоба
blob-verb-upgrade-to-reflective = Улучшить до отражающего блоба
blob-verb-remove-blob-tile = Убрать блоб
# Alerts
blob-resource-alert-name = Ресурсы ядра
blob-resource-alert-desc = Ваши ресурсы, производимые ядром и ресурсными блобами. Используйте их для расширения и создания специализированных блобов.
blob-health-alert-name = Здоровье ядра
blob-health-alert-desc = Здоровье вашего ядра. Вы умрёте, если оно упадёт до нуля.
# Greeting
blob-role-greeting =
    Вы — блоб, паразитическое космическое существо, способное уничтожать целые станции.
        Ваша цель — выжить и вырасти как можно больше.
        Вы почти неуязвимы к физическому урону, но огонь всё ещё может навредить вам.
        Используйте Alt+ЛКМ, чтобы улучшать обычные плитки блоба в усиленные, а усиленные — в отражающие.
        Обязательно размещайте ресурсные блобы для генерации ресурсов.
        Помните, что ресурсные блобы и фабрики работают только рядом с узловыми блобами или ядрами.
        Используйте + или +e в чате, чтобы общаться со своими подчинёнными при помощи коллективного разума Блоба.
blob-zombie-greeting = Вы были заражены и воскрешены спорами блоба. Теперь вам нужно помочь блобу захватить станцию. Используйте +e в чате для общения через коллективный разум Блоба.
# End round
blob-round-end-result =
    { $blobCount ->
        [one] Было одно заражение блобом.
       *[other] Было { $blobCount } заражений блобами.
    }
blob-user-was-a-blob = [color=gray]{ $user }[/color] был(а) блобом.
blob-user-was-a-blob-named = [color=White]{ $name }[/color] ([color=gray]{ $user }[/color]) был(а) блобом.
blob-was-a-blob-named = [color=White]{ $name }[/color] был(а) блобом.
preset-blob-objective-issuer-blob = [color=#33cc00]Блоб[/color]
blob-user-was-a-blob-with-objectives = [color=gray]{ $user }[/color] был(а) блобом со следующими целями:
blob-user-was-a-blob-with-objectives-named = [color=White]{ $name }[/color] ([color=gray]{ $user }[/color]) был(а) блобом со следующими целями:
blob-was-a-blob-with-objectives-named = [color=White]{ $name }[/color] был(а) блобом со следующими целями:
# Objectivies
objective-condition-blob-capture-title = Захватить станцию
objective-condition-blob-capture-description = Ваша единственная цель — захватить всю станцию. Для этого у вас должно быть не менее { $count } плиток блоба.
objective-condition-success = { $condition } | [color={ $markupColor }]Успех![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Провал![/color] ({ $progress }%)

# Admin Verbs

admin-verb-make-blob = Сделать цель носителем блоба.
admin-verb-text-make-blob = Сделать носителем блоба
# Language
language-Blob-name = Блоб
chat-language-Blob-name = Блоб
language-Blob-description = Блиб боб! Блоб блоб!
