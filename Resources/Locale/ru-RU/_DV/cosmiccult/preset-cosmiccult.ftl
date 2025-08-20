## COSMIC CULT ROUND, ANTAG & GAMEMODE TEXT

cosmiccult-announcement-sender = ???
cosmiccult-title = Космический культ
cosmiccult-description = Среди команды скрываются культисты.
roles-antag-cosmiccult-name = Космический культист
roles-antag-cosmiccult-description = Приблизьте конец всего сущего с помощью уловок и саботажа, промывая мозги тем, кто мог бы выступить против вас.
cosmiccult-gamemode-title = Космический культ
cosmiccult-gamemode-description = Сканеры фиксируют аномальное увеличение Λ-CDM. Дополнительных данных нет.
cosmiccult-vote-steward-initiator = Неизвестное
cosmiccult-vote-steward-title = Руководитель космического культа
cosmiccult-vote-steward-briefing =
    Вы - управляющий Космическим культом!
    Проследите, чтобы Монумент был установлен в надежном месте, и организуйте культ так, чтобы обеспечить вашу коллективную победу.
    Вам не разрешается инструктировать культистов о том, как использовать или расходовать их энтропию.
cosmiccult-vote-lone-steward-title = Одинокий культист
cosmiccult-vote-lone-steward-briefing =
    Вы совершенно одиноки. Но ваш долг еще не выполнен.
    Проследите, чтобы монумент был установлен в надежном месте, и завершите то, что начал культ.
cosmiccult-finale-autocall-briefing = Монумент активируется { $minutesandseconds } Соберитесь и приготовьтесь к концу.
cosmiccult-finale-ready = От Монумента исходит устрашающий свет!
cosmiccult-finale-speedup = Притяжение усиливается! Энергия разливается по всему окружению...
cosmiccult-finale-degen = Вы чувствуете, что распадаетесь на части!
cosmiccult-finale-location = Сканеры регистрируют огромный всплеск Λ-CDM { $location }!
cosmiccult-finale-cancel-begin = Сила воли вашего разума начинает разрушать ритуал...
cosmiccult-finale-beckon-begin = Шепот в глубине твоего сознания усиливается...
cosmiccult-finale-beckon-success = Ты зовешь на финальный зов.
cosmiccult-monument-powerdown = В Монументе воцаряется зловещая тишина.

## ROUNDEND TEXT

cosmiccult-roundend-cultist-count =
    { $initialCount ->
        [1] Был { $initialCount } [color=#4cabb3]Космический культист[/color].
       *[other] Были { $initialCount } [color=#4cabb3]Космическими культистами[/color].
    }
cosmiccult-roundend-entropy-count = Культ выкачал { $count } Энтропии.
cosmiccult-roundend-cultpop-count = Сектанты конвертировали { $count }% от состава экипажа.
cosmiccult-roundend-monument-stage =
    { $stage ->
        [1] Прискорбно, монумент кажется заброшенным.
        [2] Монумент набирал силу, но его завершение было недостижимо.
        [3] Монумент был завершен.
       *[other] [color=red]Что-то пошло совсем не так.[/color]
    }
cosmiccult-roundend-cultcomplete = [color=#4cabb3]Космический культ одержал полную победу![/color]
cosmiccult-roundend-cultmajor = [color=#4cabb3]Крупная победа космического культа![/color]
cosmiccult-roundend-cultminor = [color=#4cabb3]Незначительная победа космического культа![/color]
cosmiccult-roundend-neutral = [color=yellow]Нейтральное окончание![/color]
cosmiccult-roundend-crewminor = [color=green]Небольшая победа экипажа![/color]
cosmiccult-roundend-crewmajor = [color=green]Крупная победа экипажа![/color]
cosmiccult-roundend-crewcomplete = [color=green]Полная победа экипажа![/color]
cosmiccult-summary-cultcomplete = Космические культисты возвестили о конце света!
cosmiccult-summary-cultmajor = Победа космических культистов будет неизбежна.
cosmiccult-summary-cultminor = Монумент был достроен, но не в полной мере.
cosmiccult-summary-neutral = Культ доживет до следующего дня.
cosmiccult-summary-crewminor = Культ остался без управления.
cosmiccult-summary-crewmajor = Все космические культисты были уничтожены.
cosmiccult-summary-crewcomplete = Все до единого космические культисты были обращены в другую веру!
cosmiccult-elimination-shuttle-call = Согласно данным, полученным от наших датчиков дальнего действия, аномалия Λ-CDM уменьшилась. Мы благодарим вас за ваше благоразумие. На станцию автоматически был вызван аварийный шаттл для проведения процедур дезактивации и подведения итогов. эта: { $time } { $units } Пожалуйста, обратите внимание, что если психологическое воздействие аномалии незначительно, вы можете отозвать шаттл, чтобы продлить смену.
cosmiccult-elimination-announcement = Судя по данным наших датчиков дальнего действия, аномалия Λ-CDM уменьшилась. Мы благодарим вас за ваше благоразумие. Аварийный шаттл уже прибыл. Благополучно возвращайтесь в Центркомм для проведения процедур дезактивации и подведения итогов.

## BRIEFINGS

cosmiccult-role-roundstart-fluff =
    Когда вы готовитесь к очередной смене на борту очередной станции NanoTrasen, неисчислимые знания внезапно наполняют ваш разум!
    Ни с чем не сравнимое откровение. Конец циклическим сизифовым страданиям.
    Тихий занавес.

    Все, что вам нужно сделать, это начать.
cosmiccult-role-short-briefing =
    Вы - космический культист!
    Ваши цели указаны в меню персонажа.
    Подробнее о вашей роли читайте в руководстве.
cosmiccult-role-conversion-fluff =
    Когда призыв завершается, невыразимое знание внезапно наполняет ваш разум!
    Ни с чем не сравнимое откровение. Конец циклическим сизифовым страданиям.
    Тихий занавес.

    Все, что вам нужно сделать, - это возвестить об этом.
cosmiccult-role-deconverted-fluff =
    Огромная пустота заполняет твой разум. Успокаивающая, но незнакомая пустота...
    Все мысли и воспоминания о твоем пребывании в культе начинают тускнеть и размываться.
cosmiccult-role-deconverted-briefing =
    Ты более не обращен!
    Ты больше не космический культист.
cosmiccult-monument-stage1-briefing =
    Этот монумент привлек внимание многих людей.
    Он расположен { $location }!
cosmiccult-monument-stage2-briefing =
    Мощь монумента растет!
    Его влияние повлияет на реальное пространство в { $time } секунд.
cosmiccult-monument-stage3-briefing =
    монумент завершен!
    Его влияние начнет накладываться на реальное пространство в { $time } секунд.
    Это последний этап! Накопите столько энтропии, сколько сможете.

## MALIGN RIFTS

cosmiccult-rift-inuse = Ты не можешь сделать это прямо сейчас.
cosmiccult-rift-invaliduser = Вам не хватает надлежащих инструментов, чтобы справиться с этим.
cosmiccult-rift-chaplainoops = Пользуйтесь своим священным писанием.
cosmiccult-rift-alreadyempowered = Вы уже наделены силой; сила разлома была бы потрачена впустую.
cosmiccult-rift-beginabsorb = Трещина начинает сливаться с вами воедино...
cosmiccult-rift-beginpurge = Ваше посвящение начинает очищать пагубный раскол...
cosmiccult-rift-absorb = { $NAME } поглощает разлом, и зловещий свет наполняет их тело силой!
cosmiccult-rift-purge = { $NAME } устраняет пагубный раскол в реальности!

## UI / BASE POPUP

cosmiccult-ui-deconverted-title = Деконвертированный
cosmiccult-ui-converted-title = Преобразованный
cosmiccult-ui-roundstart-title = Неизвестное
cosmiccult-ui-converted-text-1 = Вы были обращены в космического культиста.
cosmiccult-ui-converted-text-2 =
    Помогайте культу в достижении его целей, сохраняя при этом его секретность.
    Участвуйте в планах своих собратьев по культу.
cosmiccult-ui-roundstart-text-1 = Ты - космический культист!
cosmiccult-ui-roundstart-text-2 =
    Помогайте культу в достижении его целей, сохраняя при этом его секретность.
    Прислушивайтесь к указаниям своего управляющего культом.
cosmiccult-ui-deconverted-text-1 = Ты больше не космический культист.
cosmiccult-ui-deconverted-text-2 =
    Вы потеряли все воспоминания, относящиеся к Космическому культу.
    Если вы будете обращены обратно, эти воспоминания вернутся.
cosmiccult-ui-popup-confirm = Подтверждать

## OBJECTIVES / CHARACTERMENU

objective-issuer-cosmiccult = [bold][color=#cae8e8]Неизвестный[/color][/bold]
objective-cosmiccult-charactermenu = Вы должны положить конец всему сущему. Выполняйте свои задания, чтобы способствовать прогрессу культа.
objective-cosmiccult-steward-charactermenu = Вы должны направлять культ, чтобы он возвестил о конце света. Наблюдайте за развитием культа и следите за его развитием.
objective-condition-entropy-title = СИФОН ЭНТРОПИИ
objective-condition-entropy-desc = Коллективно перекачивать по крайней мере { $count } энтропии из экипажа.
objective-condition-culttier-title = РАСШИРЬТЕ ВОЗМОЖНОСТИ МОНУМЕНТА
objective-condition-culttier-desc = Проследите за тем, чтобы монумент заработал на полную мощность.
objective-condition-victory-title = ВОЗВЕСТИТЬ О КОНЦЕ
objective-condition-victory-desc = Маните к себе Неизвестного и предвещайте финальный занавес.

## CHAT ANNOUNCEMENTS

cosmiccult-radio-tier1-progress = монумент манит к себе на станцию...
cosmiccult-announce-tier2-progress = Нервирующее оцепенение пронзает ваши чувства.
cosmiccult-announce-tier2-warning = Сканеры фиксируют заметное увеличение Λ-CDM! Вскоре в реальном пространстве могут появиться трещины. Пожалуйста, предупредите священника вашей станции, если увидите их.
cosmiccult-announce-tier3-progress = Дуги ноосферной энергии потрескивают в стонущем здании станции. Конец близок.
cosmiccult-announce-tier3-warning = Обнаружено критическое увеличение Λ-CDM. Зараженный персонал должен быть обезврежен на месте.
cosmiccult-announce-finale-warning = Всему экипажу станции. Аномалия Λ-CDM становится сверхкритической, приборы отказывают; неизбежен переход от ноосферы к реальности. Если вы еще не соблюдаете контрпротокол, немедленно вылетайте и вмешивайтесь. Повторяю: вмешайтесь немедленно или умрите.
cosmiccult-announce-victory-summon = ВЫСВОБОЖДАЕТСЯ ЧАСТИЦА КОСМИЧЕСКОЙ ЭНЕРГИИ.

## MISC

cosmiccult-spire-entropy = Частичка энтропии конденсируется на поверхности шпиля.
cosmiccult-entropy-inserted = Ты переправляешь { $count } энтропии, которая проникает в Монумент.
cosmiccult-entropy-unavailable = Ты не можешь сделать это прямо сейчас.
cosmiccult-astral-ascendant = { $name }, восходящий
cosmiccult-gear-pickup-rejection = { $ITEM } сопротивляется прикосновению { CAPITALIZE(THE($TARGET)) }!
cosmiccult-gear-pickup = Вы можете почувствовать, как разрываетесь, держа в руках { $ITEM }!
