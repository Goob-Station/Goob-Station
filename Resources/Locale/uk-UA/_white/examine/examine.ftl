# Poggers examine system

examine-name = Це [bold]{$name}[/bold]!
examine-name-selfaware = Це ви!
examine-can-see = Дивлячись на {OBJECT($ent)}, ви бачите:
examine-can-see-nothing = {CAPITALIZE(SUBJECT($ent))} абсолютно голий!
examine-border-line = ═════════════════════
examine-present-tex = Це [enttex id="{ $id }" size={ $size }] [bold]{$name}[/bold]!
examine-present = Це [bold]{$name}[/bold]!
examine-present-line = ═══

id-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її поясі.
head-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її голові.
eyes-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її очах.
mask-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її обличчі.
neck-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її шиї.
ears-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її вухах.
jumpsuit-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} {SUBJECT($ent)} носить.
outer-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її тілі.
suitstorage-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її плечі.
back-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її спині.
gloves-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її руках.
belt-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її поясі.
shoes-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на його/її ногах.

id-card-examine-full = • ID {CAPITALIZE(POSS-ADJ($wearer))}: [bold]{$nameAndJob}[/bold].

# Версія для себе

examine-can-see-selfaware = Дивлячись на себе, ви бачите:
examine-can-see-nothing-selfaware = Ви абсолютно голі!

id-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашому поясі.
head-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашій голові.
eyes-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на ваших очах.
mask-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашому обличчі.
neck-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашій шиї.
ears-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на ваших вухах.
jumpsuit-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} ви носите.
outer-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашому тілі.
suitstorage-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашому плечі.
back-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашій спині.
gloves-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на ваших руках.
belt-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на вашому поясі.
shoes-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} на ваших ногах.

# Огляд себе

comp-hands-examine-empty-selfaware = Ви нічого не тримаєте.
comp-hands-examine-selfaware = Ви тримаєте { $items }.

humanoid-appearance-component-examine-selfaware = Ви { INDEFINITE($age) } { $age } { $species }.
