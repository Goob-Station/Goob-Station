# SPDX-License-Identifier: AGPL-3.0-or-later
# Ребрендинг антагів для Диких земель / Оскар.

## Traitor → Корпоративний Шпигун
roles-antag-syndicate-agent-name = Корпоративний Шпигун
roles-antag-syndicate-agent-objective = Викрасти дослідження Weyland-Yutani щодо Ксеноморфів або технології Жокея — і не розкритися.
roles-antag-syndicate-agent-sleeper-name = Корпоративний агент-сплячий
roles-antag-syndicate-agent-sleeper-objective = Глибоко законспірований корпоративний шпигун, що може активуватися посеред операції.

traitor-title = Корпоративні Шпигуни
traitor-description = Серед експедиції є агенти конкуруючих корпорацій.
traitor-round-end-agent-name = корпоративний шпигун
objective-issuer-syndicate = [color=orange]Конкуруюча корпорація[/color]

traitor-role-greeting =
    Ти — [color=orange]Корпоративний Шпигун[/color] на службі {$corporation} — суперника Weyland-Yutani (Seegson, Ютані до злиття тощо).
    Замовники хочуть зразки Ксеноморфів, дані вулика або технології Інженерного Жокея з цієї бази в Диких землях.
    Цілі й кодові слова — у меню персонажа. Аплінк — для спорядження місії.
    Не дай охороні Wey-Yu тебе викрити.

traitor-role-codewords =
    Розпізнавальні фрази: [color=lightgray]
    {$codewords}.[/color]
    Використовуй обережно з іншими шпигунами.

traitor-role-allegiances =
    Твої лояльності:

## Thief
roles-antag-thief-name = Злодій чорного ринку
roles-antag-thief-objective = Поцупити цінний вантаж Wey-Yu, зразки чи зброю найманців для чорного ринку — без відкритого насильства.

thief-role-greeting-human =
    Ти — злодій чорного ринку на фронтирній базі Weyland-Yutani.
    Корпоративний вантаж, зразки Ксеноморфів і зброя найманців дорого коштують поза світом.
    Після останнього арешту тобі вживили імплант пацифізму — кради розумно й тихо.

thief-role-greeting-animal =
    Ти — клептоманська тварина.
    Кради блискуче корпоративне барахло.

thief-role-greeting-equipment =
    У тебе сумка злодійських інструментів і здатність красти непомітно. Обери спорядження й працюй у тіні.

objective-issuer-thief = [color=#746694]Чорний ринок[/color]
thief-round-end-agent-name = злодій чорного ринку

## Blob
roles-antag-blob-name = Біо-вулик
roles-antag-blob-objective = Розростай планетарну біомасу, доки база не буде поглинута.
ghost-role-information-blob-name = Біо-вулик
ghost-role-information-blob-description = Ти — невідома планетарна біомаса: ксеноморфний ріст вулика, що пожирає базу в Диких землях.
ghost-role-information-blobbernaut-name = Страж вулика
ghost-role-information-blobbernaut-description = Ти — страж біо-вулика. Захищай ядро. + або +e — мова розуму вулика.
objective-issuer-blob = Біо-вулик
guide-entry-blob = Біо-вуликова інвазія

## Cosmic Cult → Ксено-культ
cosmiccult-title = Ксено-культ
cosmiccult-description = Серед експедиції ховаються поклонники Ксеноморфів.
roles-antag-cosmiccult-name = Ксено-культист
roles-antag-cosmiccult-description = Таємно служи Імператриці. Саботуй базу, навертай слабких, готуй місце під вулик.
cosmiccult-gamemode-title = Ксено-культ
cosmiccult-gamemode-description = Біосканери ловлять аномальні феромонні сигнатури вулика. Інших даних немає.

cosmiccult-vote-steward-initiator = Імператриця
cosmiccult-vote-steward-title = Лідерство Ксено-культу
cosmiccult-vote-steward-briefing =
    Ти — Стюард Ксено-культу!
    Забезпеч безпечне місце для Монумента (вівтар вулика), організуй культ і не дай Wey-Yu виявити зараження.
    Не диктуй культистам, як витрачати Ентропію.

objective-issuer-cosmiccult = [bold][color=#7CFC00]Імператриця[/color][/bold]
objective-cosmiccult-charactermenu = Просувай претензії вулика на цей світ. Виконуй завдання для Імператриці.
objective-cosmiccult-steward-charactermenu = Керуй культом. Збільшуй вплив Імператриці, не викриваючи гніздо.

cosmiccult-role-roundstart-fluff =
    На цій фронтирній планеті Weyland-Yutani в голові відкривається щось старіше за Компанію.
    Імператриця. Досконалий організм. Вулик, що поверне Дикі землі.
    Ти годуватимеш його зсередини.

cosmiccult-role-short-briefing =
    Ти — Ксено-культист!
    Цілі — у меню персонажа.
    Деталі культових інструментів — у гайді (лор: обряди вулика).

cosmiccult-role-conversion-fluff =
    Обряд завершено. Пісня вулика затоплює думки.
    Імператриця дивиться. Досконалий організм чекає.
    Служи гнізду.

cosmiccult-role-deconverted-briefing =
    Деконвертовано!
    Ти більше не Ксено-культист.

cosmiccult-ui-roundstart-title = Імператриця
cosmiccult-ui-converted-text-1 =
    Тебе прийняли до Ксено-культу.
cosmiccult-ui-converted-text-2 =
    Допомагай культу таємно. Розширюй претензії Імператриці на базу.
cosmiccult-ui-roundstart-text-1 =
    Ти — Ксено-культист!
cosmiccult-ui-roundstart-text-2 =
    Тримай гніздо в таємниці. Слухай Стюарда.
cosmiccult-ui-deconverted-text-1 =
    Ти більше не Ксено-культист.
cosmiccult-ui-deconverted-text-2 =
    Спогади про культ згасають. Якщо тебе знову навернуть — вони повернуться.

cosmiccult-roundend-cultcomplete = [color=#7CFC00]Повна перемога Ксено-культу![/color]
cosmiccult-roundend-cultmajor = [color=#7CFC00]Велика перемога Ксено-культу![/color]
cosmiccult-roundend-cultminor = [color=#7CFC00]Мала перемога Ксено-культу![/color]
cosmiccult-summary-cultcomplete = Ксено-культ відкрив світ Імператриці!
cosmiccult-summary-cultmajor = Стюард утік із знанням вулика. Культ живе.
cosmiccult-summary-crewmajor = Усі Ксено-культисти знищені.
cosmiccult-summary-crewcomplete = Усіх Ксено-культистів деконвертовано!

guide-entry-cosmiccult = Ксено-культ

roles-antag-corporate-agent-name = Оперативник конкурента Wey-Yu
roles-antag-corporate-agent-description = Використовуй корпоративні ресурси, щоб викрасти дослідження Ксеноморфів або Жокея з бази в Диких землях.