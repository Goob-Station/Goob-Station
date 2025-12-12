## COSMIC CULT ROUND, ANTAG & GAMEMODE TEXT

cosmiccult-announcement-sender = ???
cosmiccult-title = Культ космоса
cosmiccult-description = Культисты скрываются среди экипажа.
roles-antag-cosmiccult-name = Космический культист
roles-antag-cosmiccult-description = Веди к концу всего сущего через обман и саботаж, промывая мозги тем, кто противостоит тебе.
cosmiccult-gamemode-title = Культ космоса
cosmiccult-gamemode-description = Сканеры фиксируют аномальный рост Лямбда-CDM. Дополнительных данных нет.
cosmiccult-vote-steward-initiator = Неизвестный
cosmiccult-vote-steward-title = Управление Космическими культистами
cosmiccult-vote-steward-briefing =
    Вы — управляющий Космического культа!
    Убедитесь, что Монумент установлен в безопасном месте, и организуйте культ для достижения общей победы.
    Вам запрещено инструктировать культистов о том, как использовать или тратить их энтропию.
cosmiccult-vote-lone-steward-title = Одинокий культист
cosmiccult-vote-lone-steward-briefing =
    Вы совершенно одни, но должны исполнить свой долг.
    Убедитесь, что Монумент установлен в безопасном месте, и завершите начатое культом.
cosmiccult-finale-autocall-briefing = Монумент активируется через { $minutesandseconds }! Соберитесь и приготовьтесь к концу.
cosmiccult-finale-ready = Ужасающий свет вырывается из Монумента!
cosmiccult-finale-speedup = Зов усиливается! Энергия пронизывает всё вокруг...
cosmiccult-finale-degen = Ты чувствуешь, как распадаешься!
cosmiccult-finale-location = Сканеры фиксируют огромный всплеск Лямбда-CDM в { $location }!
cosmiccult-finale-cancel-begin = Сила вашего разума начинает разрушать ритуал...
cosmiccult-finale-beckon-begin = Шёпоты в глубине вашего сознания усиливаются...
cosmiccult-finale-beckon-success = Вы призываете финальный занавес.
cosmiccult-monument-powerdown = Монумент зловеще затихает.

## ROUNDEND TEXT

cosmiccult-roundend-cultist-count =
    { $initialCount ->
        [1] Был { $initialCount } [color=#4cabb3]Космических культистов[/color].
       *[other] Было { $initialCount } [color=#4cabb3]Космических культистов[/color].
    }
cosmiccult-roundend-entropy-count = Культ поглотил { $count } энтропии.
cosmiccult-roundend-cultpop-count = Культисты составляли { $count }% от всего экипажа станции.
cosmiccult-roundend-monument-stage =
    { $stage ->
        [1] Увы, Монумент остался заброшенным.
        [2] Монумент был усилен, но до завершения не хватило времени.
        [3] Монумент был завершён.
       *[other] [color=red]Что-то пошло не так.[/color]
    }
cosmiccult-roundend-cultcomplete = [color=#4cabb3]Культ одержал полную победу![/color]
cosmiccult-roundend-cultmajor = [color=#4cabb3]Культ одержал крупную победу![/color]
cosmiccult-roundend-cultminor = [color=#4cabb3]Культ одержал малую победу![/color]
cosmiccult-roundend-neutral = [color=yellow]Нейтральный финал![/color]
cosmiccult-roundend-crewminor = [color=green]Экипаж одержал малую победу![/color]
cosmiccult-roundend-crewmajor = [color=green]Экипаж одержал крупную победу![/color]
cosmiccult-roundend-crewcomplete = [color=green]Экипаж одержал полную победу![/color]
cosmiccult-summary-cultcomplete = Культисты призвали конец всего!
cosmiccult-summary-cultmajor = Победа культистов стала неизбежной.
cosmiccult-summary-cultminor = Монумент был завершён, но не полностью активирован.
cosmiccult-summary-neutral = Культ уцелел и переживёт этот день.
cosmiccult-summary-crewminor = Культ остался без управляющего.
cosmiccult-summary-crewmajor = Все культисты были устранены.
cosmiccult-summary-crewcomplete = Каждый культист был обращён!
cosmiccult-elimination-shuttle-call = Согласно сканированию дальнего радиуса, аномалия Лямбда-CDM угасла. Благодарим вас за проявленную бдительность. Эвакуационный шаттл был вызван. Он прибудет через: { $time } { $units }. Если последствия для корпоративных активов и экипажа минимальны, вы можете отозвать шаттл для продолжения смены.
cosmiccult-elimination-announcement = Согласно сканированию дальнего радиуса, аномалия Лямбда-CDM угасла. Благодарим вас за проявленную бдительность. Эвакуационный шаттл был вызван. Возвращайтесь на станцию Центрального Командования.

## BRIEFINGS

cosmiccult-role-roundstart-fluff =
    Пока вы готовитесь к ещё одной смене на очередной станции NanoTrasen, в ваш разум врывается поток неисчислимых запретных знаний!
    Откровения, не имеющее равных. Конец циклическим, сизифовым страданиям.
    
    Всё, что вам остаётся — лишь впустить это.
cosmiccult-role-short-briefing =
    Вы — Космический культист!
    Ваши цели указаны в меню персонажа.
    Подробнее о вашей роли читайте в руководстве.
cosmiccult-role-conversion-fluff =
    По завершении ритуала, в ваш разум врывается поток неисчислимых запретных знаний!
    Откровения, не имеющие равных. Конец циклическим, сизифовым страданиям.
    
    Всё, что вам остаётся — лишь впустить это.
cosmiccult-role-deconverted-fluff =
    Ваш разум захлёстывает великая пустота. Утешительная, но незнакомая пустота...
    Все мысли и воспоминания о времени, проведённом в культе, начинают исчезать и размываться.
cosmiccult-role-deconverted-briefing =
    Обращение!
    Вы больше не Космический культист.
cosmiccult-monument-stage1-briefing =
    Монумент был призван.
    Он находится в { $location }!
cosmiccult-monument-stage2-briefing =
    Монумент набирает силу!
    Его влияние затронет реальность через { $time } секунд.
cosmiccult-monument-stage3-briefing =
    Монумент был завершён!
    Его влияние начнёт пересекаться с реальностью через { $time } секунд.
    Это финальный этап! Соберите столько энтропии, сколько сможете.

## MALIGN RIFTS

cosmiccult-rift-inuse = Вы не можете сделать это прямо сейчас.
cosmiccult-rift-invaliduser = У вас нет нужных инструментов для этого.
cosmiccult-rift-chaplainoops = Используйте своё священное писание.
cosmiccult-rift-alreadyempowered = Вы уже наделены силой; сила разлома была бы потрачена зря.
cosmiccult-rift-beginabsorb = Разлом начинает сливаться с вами...
cosmiccult-rift-beginpurge = Ваша освящённость начинает очищать зловещий разлом...
cosmiccult-rift-absorb = { $NAME } поглощает разрыв, и зловещий свет наполняет его тело!
cosmiccult-rift-purge = { $NAME } изгоняет зловещий разлом из реальности!

## UI / BASE POPUP

cosmiccult-ui-deconverted-title = Обращение
cosmiccult-ui-converted-title = Конверсия
cosmiccult-ui-roundstart-title = Неизвестно
cosmiccult-ui-converted-text-1 = Вы были обращены в Космического культиста.
cosmiccult-ui-converted-text-2 =
    Помогайте культу в достижении целей, при этом, сохраните его в тайне.
    Сотрудничайте с другими культистами.
cosmiccult-ui-roundstart-text-1 = Вы — Культист Космоса!
cosmiccult-ui-roundstart-text-2 =
    Помогайте культу в достижении целей, при этом, сохраните его в тайне.
    Следуйте указаниям Управляющего культом.
cosmiccult-ui-deconverted-text-1 = Вы больше не Космический культист.
cosmiccult-ui-deconverted-text-2 =
    Вы утратили все воспоминания, связанные с Космическим культом.
    Если вас вновь обратят, воспоминания вернутся.
cosmiccult-ui-popup-confirm = Подтвердить

## OBJECTIVES / CHARACTERMENU

objective-issuer-cosmiccult = [bold][color=#cae8e8]Неизвестно[/color][/bold]
objective-cosmiccult-charactermenu = Вы должны привести всё к концу. Выполняйте задания, чтобы продвигать культ.
objective-cosmiccult-steward-charactermenu = Вы должны направлять культ к концу всего сущего. Контролируйте и обеспечьте прогресс культа.
objective-condition-entropy-title = ПОГЛОТИТЕ ЭНТРОПИЮ
objective-condition-entropy-desc = Совместно поглотите как минимум { $count } энтропии с экипажа станции.
objective-condition-culttier-title = УКРЕПИТЕ МОHУМЕНТ
objective-condition-culttier-desc = Сделайте всё возможное, чтобы Монумент достиг своей полной силы.
objective-condition-victory-title = НАЧНИТЕ КОНЕЦ
objective-condition-victory-desc = Призовите ЕГО, и обреките конец для всего сущего.

## CHAT ANNOUNCEMENTS

cosmiccult-radio-tier1-progress = Монумент был призван на станцию...
cosmiccult-announce-tier2-progress = Тревожное оцепенение пробирает вас насквозь.
cosmiccult-announce-tier2-warning = Сканеры фиксируют значительное увеличение Лямбда-CDM! Разломы реальности могут появиться в ближайшее время. Пожалуйста, уведомите священника станции.
cosmiccult-announce-tier3-progress = Дуговые разряды ноосферной энергии потрескивают по скрипящей структуре станции. Конец близок.
cosmiccult-announce-tier3-warning = Зафиксировано критическое увеличение Лямбда-CDM. Инфицированный персонал подлежит немедленной нейтрализации.
cosmiccult-announce-finale-warning = Внимание всему экипажу. Аномалия Лямбда-CDM переходит в сверхкритическую фазу, приборы отказывают; переход горизонта событий из ноосферы в реальность НЕИЗБЕЖЕН. Если вы не задействованы в контрмерах — немедленно вмешайтесь. Повторяю: вмешайтесь немедленно или погибнете.
cosmiccult-announce-victory-summon = ЧАСТИЦА КОСМИЧЕСКОЙ СИЛЫ ПРИЗВАНА.

## MISC

cosmiccult-spire-entropy = С поверхности шпиля конденсируется частица энтропии.
cosmiccult-entropy-inserted = Вы вливаете { $count } энтропии в Монумент.
cosmiccult-entropy-unavailable = Вы не можете сделать это прямо сейчас.
cosmiccult-astral-ascendant = { $name }, Вознесённый
cosmiccult-gear-pickup-rejection = { $ITEM } сопротивляется прикосновению { CAPITALIZE($TARGET) }!
cosmiccult-gear-pickup = Вы чувствуете, как ваше Я расплетается, пока вы держите { $ITEM }!
cult-alert-recall-shuttle = Обнаружены высокие концентрации Лямбда-CDM неизвестного происхождения на станции. Все аномальные присутствия должны быть устранены до эвакуации.
