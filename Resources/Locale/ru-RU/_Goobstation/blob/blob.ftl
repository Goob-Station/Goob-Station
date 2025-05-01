

objective-issuer-blob = Блоб
ghost-role-information-blobbernaut-name = Блоббернаут
ghost-role-information-blobbernaut-description = Вы - Блоббернаут. Вы должны защищать ядро блоба.
ghost-role-information-blob-name = Блоб
ghost-role-information-blob-description = Вы - носитель блоба. Поглотите станцию.
roles-antag-blob-name = Блоб
roles-antag-blob-objective = Достичь критической массы.
guide-entry-blob = Блоб
# Popups
blob-target-normal-blob-invalid = Неправильный тип блоба, выберите нормальный блоб.
blob-target-factory-blob-invalid = Неправильный тип блоба, выберите фабричный блоб.
blob-target-node-blob-invalid = Неправильный тип блоба, выберите блоб узла.
blob-target-close-to-resource = Слишком близко к другому ресурсному блобу.
blob-target-nearby-not-node = Поблизости нет узла или ресурсного блоба.
blob-target-close-to-node = Слишком близко к другому узлу.
blob-target-already-produce-blobbernaut = Эта фабрика уже произвела блоббернаута.
blob-cant-split = Вы не можете разделить ядро блоба.
blob-not-have-nodes = У вас нет узлов.
blob-not-enough-resources = Недостаточно ресурсов.
blob-help = Только Бог может помочь вам.
blob-swap-chem = В разработке.
blob-mob-attack-blob = Вы не можете атаковать блоба.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = Вы умираете, находясь не на блоб-плитке.
carrier-blob-alert = У вас осталось { $second } секунд до превращения.
blob-mob-zombify-second-start = { $pod } начинает превращать вас в зомби.
blob-mob-zombify-third-start = { $pod } начинает превращать { $target } в зомби.
blob-mob-zombify-second-end = { $pod } превращает вас в зомби.
blob-mob-zombify-third-end = { $pod } превращает { $target } в зомби.
blobberaut-factory-destroy = уничтожить фабрику
blob-target-already-connected = Рядом слишком много блобов данного типа!
# UI
blob-chem-swap-ui-window-name = Замена химикатов
blob-chem-reactivespines-info =
    Реактивные шипы
    Наносит 25 единиц грубого урона.
blob-chem-blazingoil-info =
    Пылающее масло
    Наносит 15 единиц урона от ожогов и поджигает цели.
    Делает вас уязвимым к воде.
blob-chem-regenerativemateria-info =
    Регенеративная материя
    Наносит 6 единиц грубого урона и 15 единиц урона от токсинов.
    Ядро блоба восстанавливает здоровье в 10 раз быстрее обычного и генерирует 1 дополнительный ресурс.
blob-chem-explosivelattice-info =
    Взрывная решетка
    Наносит 5 единиц урона от ожогов и взрывает цель, нанося 10 единиц грубого урона.
    Споры взрываются при смерти.
    Вы становитесь невосприимчивы к взрывам.
    Вы получаете на 50% больше урона от ожогов и электрошока.
blob-chem-electromagneticweb-info =
    Электромагнитная паутина
    Наносит 20 ед. урона от ожогов, с вероятностью 20% вызывает ЭМИ-импульс при атаке.
    Плитки шара вызывают ЭМИ-импульс при уничтожении.
    Вы получаете на 25% больше грубого и теплового урона.
blob-alert-out-off-station = Блоб был удален, потому что был найден за пределами станции!
# Announcment
blob-alert-recall-shuttle = Аварийный шаттл не может быть отправлен, пока на станции присутствует биологическая опасность пятого уровня.
blob-alert-detect = Подтверждена вспышка биологической опасности пятого уровня на борту станции. Весь персонал должен сдержать вспышку.
blob-alert-critical = Уровень биологической опасности критический, на станцию отправлены коды ядерной аутентификации. Центральное командование приказывает всему оставшемуся персоналу активировать механизм самоуничтожения.
blob-alert-critical-NoNukeCode = Уровень биологической опасности критический. Центральное командование приказывает всему оставшемуся персоналу найти укрытие и ожидать возвращения.
# Actions
blob-create-factory-action-name = Поместить фабричный блоб (80)
blob-create-factory-action-desc = Превращает выбранный обычный блоб в фабричный, который будет производить до 3 спор и блоббернаута, если поместить его рядом с ядром или узлом.
blob-create-resource-action-name = Поместить ресурсный блоб (60)
blob-create-resource-action-desc = Превращает выбранный обычный блоб в блоб ресурсов, который будет генерировать ресурсы, если его поместить рядом с ядром или узлом.
blob-create-node-action-name = Поместить блоб узла (50)
blob-create-node-action-desc =
    Превращает выбранный обычный блоб в блоб узла.
    Узловой блоб активирует эффекты фабрики и ресурсных блобов, лечит другие блобы и медленно расширяется, разрушая стены и создавая нормальные блобы.
blob-produce-blobbernaut-action-name = Создать блоббернаута (60)
blob-produce-blobbernaut-action-desc = Создает блоббернаута на выбранной фабрике. Каждая фабрика может сделать это только один раз. Блоббернаут получает урон за пределами тайлов блобов и исцеляется, когда находится рядом с узлами.
blob-split-core-action-name = Разделить ядро (400)
blob-split-core-action-desc = Вы можете сделать это только один раз. Превращает выбранный узел в независимое ядро, которое будет действовать самостоятельно.
blob-swap-core-action-name = Переместить ядро (200)
blob-swap-core-action-desc = Поменяет местоположение вашего ядра и выбранного узла.
blob-teleport-to-core-action-name = Перейти к ядру (0)
blob-teleport-to-core-action-desc = Телепортирует вас к вашему ядру.
blob-teleport-to-node-action-name = Перейти к узлу (0)
blob-teleport-to-node-action-desc = Телепортирует вас к случайному узлу блоба.
blob-help-action-name = Помощь
blob-help-action-desc = Получить основную информацию об игре за блоба.
blob-swap-chem-action-name = Поменять химикаты (70)
blob-swap-chem-action-desc = Позволяет поменять текущий химикат.
blob-carrier-transform-to-blob-action-name = Превратиться в блоб
blob-carrier-transform-to-blob-action-desc = Мгновенно разрушает ваше тело и создает ядро сгустка. Убедитесь, что вы стоите на станции, иначе вы просто исчезнете.
blob-downgrade-action-name = понизить уровень блоба(0)
blob-downgrade-action-desc = Превращает выбранную плитку обратно в обычный блоб для установки других типов клеток.
# Ghost role
blob-carrier-role-name = Переносчик блобов
blob-carrier-role-desc = Существо, зараженное блобами.
blob-carrier-role-rules =
    Вы - антагонист. У вас есть 4 минуты, прежде чем вы превратитесь в блоб.
    Используйте это время, чтобы найти безопасное место на станции. Имейте в виду, что сразу после превращения вы будете очень слабы.
blob-carrier-role-greeting = Вы являетесь носителем блоба. Найдите укромное место на станции и превратитесь в Блоба. Превратите станцию в массу, а ее обитателей - в своих слуг. Мы все - Блобы.
# Verbs
blob-pod-verb-zombify = Зомбировать
blob-verb-upgrade-to-strong = Усилить плитку блоба
blob-verb-upgrade-to-reflective = Перейти на отражающий блоб
blob-verb-remove-blob-tile = Удалить блоб
# Alerts
blob-resource-alert-name = Ресурсы ядра
blob-resource-alert-desc = Ваши ресурсы, произведенные блобами ядра и ресурсов. Используйте их для расширения и создания специальных блоков.
blob-health-alert-name = Здоровье ядра
blob-health-alert-desc = Здоровье вашего ядра. Вы умрете, если оно достигнет нуля.
# Greeting
blob-role-greeting =
    Вы - блоб - паразитическое космическое существо, способное уничтожать целые станции.
        Ваша цель - выжить и вырасти как можно больше.
    	Вы почти неуязвимы для физического урона, но тепло все же может навредить вам.
        Используйте Alt+LMB, чтобы улучшить обычные плитки блобов до сильных блобов, а сильные блобы - до отражающих.
    	Обязательно размещайте ресурсные блобы, чтобы генерировать ресурсы.
        Помните, что ресурсные блобы и фабрики будут работать только рядом с узловыми блобами или ядрами.
blob-zombie-greeting = Вы были заражены и выращены спорой блоба. Теперь вы должны помочь блобу захватить станцию.
# End round
blob-round-end-result =
    { $blobCount ->
        [one] Было одно заражение блоба.
       *[other] Было { $blobCount } блобов.
    }
blob-user-was-a-blob = [color=gray]{ $user }[/color] был блобом.
blob-user-was-a-blob-named = [color=White]{ $name }[/color] ([color=gray]{ $user }[/color]) был блобом.
blob-was-a-blob-named = [color=White]{ $name }[/color] был блобом.
preset-blob-objective-issuer-blob = [color=#33cc00]Blob[/color]
blob-user-was-a-blob-with-objectives = [color=gray]{ $user }[/color] был блобом, у которого были следующие цели:
blob-user-was-a-blob-with-objectives-named = [color=White]{ $name }[/color] ([color=gray]{ $user }[/color]) был блобом, у которого были следующие цели:
blob-was-a-blob-with-objectives-named = [color=White]{ $name }[/color] был блобом, у которого были следующие цели:
# Objectivies
objective-condition-blob-capture-title = Захватить станцию
objective-condition-blob-capture-description = Ваша единственная цель - захватить всю станцию. У вас должно быть не менее { $count } блобов.
objective-condition-success = { $condition } | [color={ $markupColor }]Успех![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Неудача![/color] ({ $progress }%)
