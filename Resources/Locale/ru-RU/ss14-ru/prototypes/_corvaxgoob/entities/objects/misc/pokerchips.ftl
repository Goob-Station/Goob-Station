ent-PokerChipBase = покерная фишка
    .desc = Простая покерная фишка
ent-PokerChip5 = { ent-PokerChipBase }
    .desc = Простая покерная фишка значением 5.
    .suffix = Значение 5, 10
ent-PokerChip5One = { ent-PokerChip5 }
    .desc = { ent-PokerChip5.desc }
    .suffix = Значение 5, 1
ent-PokerChip5Five = { ent-PokerChip5 }
    .desc = { ent-PokerChip5.desc }
    .suffix = Значение 5, 5
ent-PokerChip25 = { ent-PokerChipBase }
    .desc = Простая покерная фишка значением 25.
    .suffix = Значение 25, 10
ent-PokerChip25One = { ent-PokerChip25 }
    .desc = { ent-PokerChip25.desc }
    .suffix = Значение 25, 1
ent-PokerChip25Five = { ent-PokerChip25 }
    .desc = { ent-PokerChip25.desc }
    .suffix = Значение 25, 5
ent-PokerChip50 = { ent-PokerChipBase }
    .desc = Простая покерная фишка значением 50.
    .suffix = Значение 50, 10
ent-PokerChip50One = { ent-PokerChip50 }
    .desc = { ent-PokerChip50.desc }
    .suffix = Значение 50, 1
ent-PokerChip50Five = { ent-PokerChip50 }
    .desc = { ent-PokerChip50.desc }
    .suffix = Значение 50, 5
ent-PokerChip100 = { ent-PokerChipBase }
    .desc = Простая покерная фишка значением 100.
    .suffix = Значение 100, 10
ent-PokerChip100One = { ent-PokerChip100 }
    .desc = { ent-PokerChip100.desc }
    .suffix = Значение 100, 1
ent-PokerChip100Five = { ent-PokerChip100 }
    .desc = { ent-PokerChip100.desc }
    .suffix = Значение 100, 5
stack-poker-chip-5 =
    { $amount ->
        [1] покерная фишка
       *[other] покерные фишки
    }
stack-poker-chip-25 = { stack-poker-chip-5 }
stack-poker-chip-50 = { stack-poker-chip-5 }
stack-poker-chip-100 = { stack-poker-chip-5 }
