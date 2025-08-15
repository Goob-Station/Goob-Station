## COSMIC CULT ROUND, ANTAG & GAMEMODE TEXT

cosmiccult-announcement-sender = ???
cosmiccult-title = Культ Космоса
cosmiccult-description = Культисты скрываются среди экипажа.
roles-antag-cosmiccult-name = Космический Культист
roles-antag-cosmiccult-description = Веди к концу всего сущего через обман и саботаж, промывая мозги тем, кто противостоит тебе.
cosmiccult-gamemode-title = Культ Космоса
cosmiccult-gamemode-description = Сканеры фиксируют аномальный рост Λ-CDM. Дополнительных данных нет.
cosmiccult-vote-steward-initiator = Неизвестный
cosmiccult-vote-steward-title = Управление Культом Космоса
cosmiccult-vote-steward-briefing =
    Ты — Управляющий Космического Культа!
    Убедись, что Монумент установлен в безопасном месте, и организуй культ для достижения общей победы.
    Тебе запрещено инструктировать культистов о том, как использовать или тратить их Энтропию.
cosmiccult-vote-lone-steward-title = Одинокий Культист
cosmiccult-vote-lone-steward-briefing =
    Ты совершенно один. Но твой долг не окончен.
    Убедись, что Монумент установлен в безопасном месте, и заверши начатое культом.
cosmiccult-finale-autocall-briefing = Монумент активируется через { $minutesandseconds }! Соберитесь и приготовьтесь к концу.
cosmiccult-finale-ready = Ужасающий свет вырывается из Монумента!
cosmiccult-finale-speedup = Зов усиливается! Энергия пронизывает всё вокруг...
cosmiccult-finale-degen = Ты чувствуешь, как распадаешься!
cosmiccult-finale-location = Сканеры фиксируют огромный всплеск Λ-CDM в { $location }!
cosmiccult-finale-cancel-begin = Сила твоего разума начинает разрушать ритуал...
cosmiccult-finale-beckon-begin = Шепоты в глубине твоего сознания усиливаются...
cosmiccult-finale-beckon-success = Ты призываешь финальный занавес.
cosmiccult-monument-powerdown = Монумент зловеще затихает.

## ROUNDEND TEXT

cosmiccult-roundend-cultist-count =
    { $initialCount ->
        [1] Был { $initialCount } [color=#4cabb3]Культистом Космоса[/color].
       *[other] Было { $initialCount } [color=#4cabb3]Культистов Космоса[/color].
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
cosmiccult-roundend-cultminor = [color=#4cabb3]Культ одержал незначительную победу![/color]
cosmiccult-roundend-neutral = [color=yellow]Нейтральный финал![/color]
cosmiccult-roundend-crewminor = [color=green]Экипаж одержал незначительную победу![/color]
cosmiccult-roundend-crewmajor = [color=green]Экипаж одержал крупную победу![/color]
cosmiccult-roundend-crewcomplete = [color=green]Экипаж одержал полную победу![/color]
cosmiccult-summary-cultcomplete = Культисты призвали конец всему!
cosmiccult-summary-cultmajor = Победа культистов стала неизбежной.
cosmiccult-summary-cultminor = Монумент был завершён, но не полностью активирован.
cosmiccult-summary-neutral = Культ уцелел и переживёт этот день.
cosmiccult-summary-crewminor = Культ остался без руководства.
cosmiccult-summary-crewmajor = Все культисты были устранены.
cosmiccult-summary-crewcomplete = Каждый культист был деконвертирован!
cosmiccult-elimination-shuttle-call = Согласно сканам с дальнего радиуса, аномалия Λ-CDM угасла. Благодарим вас за проявленную бдительность. На станцию автоматически вызван аварийный шаттл.. ОЖИДАЕМОЕ ВРЕМЯ ПРИБЫТИЯ: { $time } { $units }. Если последствия для корпоративных активов и экипажа минимальны, вы можете отозвать шаттл для продолжения смены.
cosmiccult-elimination-announcement = Согласно сканам с дальнего радиуса, аномалия Λ-CDM угасла. Благодарим вас за проявленную бдительность. Аварийный шаттл уже направляется на станцию. Возвращайтесь на станцию Центрального Командования.

## BRIEFINGS

cosmiccult-role-roundstart-fluff =
    Пока вы готовитесь к ещё одной смене на очередной станции Nanotrasen, в ваш разум внезапно хлынул поток запретных знаний!
    Откровения, не имеющее равных. Конец циклическим, истощающим страданиям.
    
    Всё, что нужно — впустить его.
cosmiccult-role-short-briefing =
    Вы — Культист Космоса!
    Ваши цели указаны в меню персонажа.
    Подробнее о роли можно узнать в справочнике.
cosmiccult-role-conversion-fluff =
    По завершении Ритуала в ваш разум внезапно хлынул поток запретных знаний!
    Откровения, не имеющие равных. Конец циклическим, истощающим страданиям.
    Мягкий последний аккорд.
    
    Всё, что нужно — впустить его.
cosmiccult-role-deconverted-fluff =
    Ваш разум захлёстывает великая пустота. Уютная, но незнакомая пустота...
    Все мысли и воспоминания о времени в культе начинают угасать и расплываться.
cosmiccult-role-deconverted-briefing =
    Деконвертация!
    Вы больше не Культист Космоса.
cosmiccult-monument-stage1-briefing =
    Монумент был призван.
    Он находится в { $location }!
cosmiccult-monument-stage2-briefing =
    Монумент набирает силу!
    Его влияние затронет реальность через { $time } секунд.
cosmiccult-monument-stage3-briefing =
    Монумент был завершён!
    Его влияние начнёт пересекаться с реальностью через { $time } секунд.
    Это финальный этап! Соберите как можно больше энтропии.

## MALIGN RIFTS

cosmiccult-rift-inuse = Сейчас вы не можете это сделать.
cosmiccult-rift-invaliduser = У вас нет нужных инструментов для этого.
cosmiccult-rift-chaplainoops = Возьмите в руки священное писание.
cosmiccult-rift-alreadyempowered = Вы уже наделены силой; энергия разрыва будет потрачена впустую.
cosmiccult-rift-beginabsorb = Разрыв начинает сливаться с вами...
cosmiccult-rift-beginpurge = Ваша освящённость начинает очищать зловещий разрыв...
cosmiccult-rift-absorb = { $NAME } поглощает разрыв, и зловещий свет наполняет его тело!
cosmiccult-rift-purge = { $NAME } изгоняет зловещий разрыв из реальности!

## UI / BASE POPUP

cosmiccult-ui-deconverted-title = Деконверсия
cosmiccult-ui-converted-title = Конверсия
cosmiccult-ui-roundstart-title = Неизвестное
cosmiccult-ui-converted-text-1 = Вы были обращены в Культ Космоса.
cosmiccult-ui-converted-text-2 =
    Помогайте культу в достижении целей, сохраняя при этом его тайну.
    Сотрудничайте с другими культистами.
cosmiccult-ui-roundstart-text-1 = Вы — Культист Космоса!
cosmiccult-ui-roundstart-text-2 =
    Помогайте культу в достижении целей, сохраняя при этом его тайну.
    Следуйте указаниям Руководителя культа.
cosmiccult-ui-deconverted-text-1 = Вы больше не Культист Космоса.
cosmiccult-ui-deconverted-text-2 =
    Вы утратили все воспоминания, связанные с Культом Космоса.
    Если вас вновь обратят, воспоминания вернутся.
cosmiccult-ui-popup-confirm = Подтвердить

## OBJECTIVES / CHARACTERMENU

objective-issuer-cosmiccult = [bold][color=#cae8e8]Неизвестное[/color][/bold]
objective-cosmiccult-charactermenu = Вы должны привести всё к концу. Выполняйте задания, чтобы продвигать культ.
objective-cosmiccult-steward-charactermenu = Вы должны направлять культ к завершению всего сущего. Контролируйте и обеспечьте прогресс культа.
objective-condition-entropy-title = ИСТОЧАЙТЕ ЭНТРОПИЮ
objective-condition-entropy-desc = Совместно источите как минимум { $count } единиц энтропии с экипажа.
objective-condition-culttier-title = УКРЕПИТЕ МОHУМЕНТ
objective-condition-culttier-desc = Обеспечьте, чтобы Монумент достиг полной силы.
objective-condition-victory-title = НАЧНИТЕ КОНЕЦ
objective-condition-victory-desc = Призовите ЕГО и начните и начните конец для всего сущего.

## CHAT ANNOUNCEMENTS

cosmiccult-radio-tier1-progress = Монумент был вызван на станцию...
cosmiccult-announce-tier2-progress = По телу пробегает беспокойное онемение.
cosmiccult-announce-tier2-warning = Сканеры фиксируют значительное увеличение Λ-CDM! Возможно, скоро появятся разрывы в реальности. Сообщите об этом вашему священнику.
cosmiccult-announce-tier3-progress = Дуговые разряды ноосферной энергии пронизывают скрежещущую станцию. Конец близок.
cosmiccult-announce-tier3-warning = Зафиксировано критическое повышение Λ-CDM. Инфицированный персонал подлежит немедленной нейтрализации.
cosmiccult-announce-finale-warning = Внимание всему экипажу. Аномалия Λ-CDM переходит в сверхкритическую фазу, приборы отказывают; переход горизонта событий из ноосферы в реальность НЕИЗБЕЖЕН. Если вы не задействованы в контрмерах — немедленно вмешайтесь. Повторяю: вмешайтесь немедленно или погибнете.
cosmiccult-announce-victory-summon = ЧАСТИЦА КОСМИЧЕСКОЙ СИЛЫ ПРИЗВАНА.

## MISC

cosmiccult-spire-entropy = С поверхности шпиля конденсируется частица энтропии.
cosmiccult-entropy-inserted = Вы вливаете { $count } единиц энтропии в Монумент.
cosmiccult-entropy-unavailable = Сейчас вы не можете это сделать.
cosmiccult-astral-ascendant = { $name }, Вознесшийся
cosmiccult-gear-pickup-rejection = { $ITEM } сопротивляется прикосновению { CAPITALIZE(THE($TARGET)) }!
cosmiccult-gear-pickup = Вы чувствуете, как ваше Я расплетается, пока вы держите { $ITEM }!
cult-alert-recall-shuttle = Обнаружены высокие концентрации Λ-CDM неизвестного происхождения на станции. Все аномальные присутствия должны быть устранены до разрешения эвакуации.
