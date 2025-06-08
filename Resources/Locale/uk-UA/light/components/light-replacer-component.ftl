
### Interaction Messages

# Shown when player tries to replace light, but there is no lights left
comp-light-replacer-missing-light = Не залишилося ламп в {THE($light-replacer)}.

# Shown when player inserts light bulb inside light replacer
comp-light-replacer-insert-light = Ви вставляєте {$bulb} в {THE($light-replacer)}.

# Показується, коли гравець намагається вставити в light replacer brolen лампочку
comp-light-replacer-insert-broken-light = Не можна вставляти розбиті лампочки!

# З'являється, коли гравець поповнює запаси світла з лайтбоксу
comp-light-replacer-refill-from-storage = Ви заправили {THE($light-replacer)}.

### Вивчити

comp-light-replacer-no-lights = Він порожній.
comp-light-replacer-has-lights = Він містить наступне:
comp-light-replacer-light-listing = {$amount ->
    [one] [color=yellow]{$amount}[/color] [color=gray]{$name}[/color]
    *[other] [color=yellow]{$amount}[/color] [color=gray]{$name}s[/color]
}
