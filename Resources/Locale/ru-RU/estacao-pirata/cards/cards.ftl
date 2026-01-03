# SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 RadsammyT <32146976+RadsammyT@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

card-examined = Это { $target }.
cards-verb-shuffle = Перемешать
card-verb-shuffle-success = Карты перемешаны
cards-verb-draw = Вытянуть карту
cards-verb-flip = Перевернуть карты
card-verb-join = Соединить карты
card-verb-organize-success =
    Карты перевёрнуты лицевой стороной { $facedown ->
        [true] вниз
       *[false] вверх
    }
cards-verb-organize-up = Перевернуть карты лицом вверх
cards-verb-organize-down = Перевернуть карты лицом вниз
cards-verb-pickcard = Взять карту
card-stack-examine =
    { $count ->
        [one] В этой стопке { $count } карта.
       *[other] В этой стопке { $count } карт.
    }
cards-stackquantitychange-added = Добавлена карта (Всего карт: { $quantity })
cards-stackquantitychange-removed = Убрана карта (Всего карт: { $quantity })
cards-stackquantitychange-joined = Стопки объединены (Всего карт: { $quantity })
cards-stackquantitychange-split = Стопка разделена (Всего карт: { $quantity })
cards-stackquantitychange-unknown = Количество карт в стопке изменилось (Всего карт: { $quantity })
cards-verb-convert-to-deck = Превратить в колоду
cards-verb-split = Разделить пополам
card-base-name = карта
card-deck-name = колода карт
card-sc-2-clubs = 2 крести
card-sc-3-clubs = 3 крести
card-sc-4-clubs = 4 крести
card-sc-5-clubs = 5 крести
card-sc-6-clubs = 6 крести
card-sc-7-clubs = 7 крести
card-sc-8-clubs = 8 крести
card-sc-9-clubs = 9 крести
card-sc-10-clubs = 10 крести
card-sc-ace-clubs = туз крести
card-sc-jack-clubs = валет крести
card-sc-king-clubs = король крести
card-sc-queen-clubs = дама крести
card-sc-2-diamonds = 2 бубен
card-sc-3-diamonds = 3 бубен
card-sc-4-diamonds = 4 бубен
card-sc-5-diamonds = 5 бубен
card-sc-6-diamonds = 6 бубен
card-sc-7-diamonds = 7 бубен
card-sc-8-diamonds = 8 бубен
card-sc-9-diamonds = 9 бубен
card-sc-10-diamonds = 10 бубен
card-sc-ace-diamonds = туз бубен
card-sc-jack-diamonds = валет бубен
card-sc-king-diamonds = король бубен
card-sc-queen-diamonds = дама бубен
card-sc-2-hearts = 2 черви
card-sc-3-hearts = 3 черви
card-sc-4-hearts = 4 черви
card-sc-5-hearts = 5 черви
card-sc-6-hearts = 6 черви
card-sc-7-hearts = 7 черви
card-sc-8-hearts = 8 черви
card-sc-9-hearts = 9 черви
card-sc-10-hearts = 10 черви
card-sc-ace-hearts = туз черви
card-sc-jack-hearts = валет черви
card-sc-king-hearts = король черви
card-sc-queen-hearts = дама черви
card-sc-2-spades = 2 пики
card-sc-3-spades = 3 пики
card-sc-4-spades = 4 пики
card-sc-5-spades = 5 пики
card-sc-6-spades = 6 пики
card-sc-7-spades = 7 пики
card-sc-8-spades = 8 пики
card-sc-9-spades = 9 пики
card-sc-10-spades = 10 пики
card-sc-ace-spades = туз пики
card-sc-jack-spades = валет пики
card-sc-king-spades = король пики
card-sc-queen-spades = дама пики
card-sc-joker = джокер
container-sealed = На нём голографическая пломба. При открытии она рассеется.
container-unsealed = Печать, наложенная на него, рассеивается.
