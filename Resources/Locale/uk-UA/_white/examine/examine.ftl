# Poggers examine system

examine-name = Це [bold]{$name}[/bold]!
examine-name-selfaware = Це ви!
examine-can-see = Оглядаючи {OBJECT($ent)}, ви бачите:
examine-can-see-nothing = {CAPITALIZE(SUBJECT($ent))} абсолютно голий!
examine-border-line = ═════════════════════
examine-present-tex = Це [enttex id=""{ $id }"" size={ $size }] [bold]{$name}[/bold]!
examine-present = Це [bold]{$name}[/bold]!
examine-present-line = ═══

id-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} поясі.
head-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} голові.
eyes-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} очах.
mask-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} обличчі.
neck-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} шиї.
ears-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} вухах.
jumpsuit-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} {SUBJECT($ent)} носить.
outer-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} тілі.
suitstorage-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} плечі.
back-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} спині.
gloves-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} руках.
belt-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} поясі.
shoes-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на {POSS-ADJ($ent)} ногах.

id-card-examine-full = • {CAPITALIZE(POSS-ADJ($wearer))} ID: [bold]{$nameAndJob}[/bold].

# Версія для себе

examine-can-see-selfaware = Оглядаючи себе, ви бачите:
examine-can-see-nothing-selfaware = Ви абсолютно голі!

id-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашому поясі.
head-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашій голові.
eyes-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на ваших очах.
mask-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашому обличчі.
neck-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашій шиї.
ears-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на ваших вухах.
jumpsuit-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} ви носите.
outer-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашому тілі.
suitstorage-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашому плечі.
back-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашій спині.
gloves-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на ваших руках.
belt-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на вашому поясі.
shoes-examine-selfaware = • Ваш { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id=""{ $id }"" size={ $size }][bold]{$item}[/bold]
} на ваших ногах.

# Огляд себе

comp-hands-examine-empty-selfaware = Ви нічого не тримаєте.
comp-hands-examine-selfaware = Ви тримаєте { $items }.

humanoid-appearance-component-examine-selfaware = Ви { INDEFINITE($age) } { $age } { $species }.
