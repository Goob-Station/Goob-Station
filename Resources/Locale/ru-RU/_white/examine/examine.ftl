# Poggers examine system

examine-name = Это [bold]{ $name }[/bold]!
examine-name-selfaware = Это вы!
examine-can-see = Посмотрев на { OBJECT($ent) }, вы можете увидеть:
examine-can-see-nothing = { CAPITALIZE(SUBJECT($ent)) } без одежды!
examine-border-line = ═════════════════════
examine-present-tex = Это [enttex id="{ $id }" size={ $size }] [bold]{ $name }[/bold]!
examine-present = Это [bold]{ $name }[/bold]!
examine-present-line = ═══
id-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на поясе { POSS-ADJ($ent) }.
head-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на голове { POSS-ADJ($ent) }.
eyes-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на глазах { POSS-ADJ($ent) }.
mask-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на лице { POSS-ADJ($ent) }.
neck-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на шее { POSS-ADJ($ent) }.
ears-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на ушах { POSS-ADJ($ent) }.
jumpsuit-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } { SUBJECT($ent) }.
outer-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на теле { POSS-ADJ($ent) }.
suitstorage-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на плечах { POSS-ADJ($ent) }.
back-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на спине { POSS-ADJ($ent) }.
gloves-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на руках { POSS-ADJ($ent) }.
belt-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на поясе { POSS-ADJ($ent) }.
shoes-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на стопах { POSS-ADJ($ent) }.
id-card-examine-full = • { CAPITALIZE(POSS-ADJ($wearer)) } ID: [bold]{ $nameAndJob }[/bold].

# Selfaware version

examine-can-see-selfaware = Посмотрев на себя, вы можете увидеть:
examine-can-see-nothing-selfaware = Вы абсолютно без одежды!
id-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на поясе.
head-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на голове.
eyes-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на глазах.
mask-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на лице.
neck-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на шее.
ears-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на глазах.
jumpsuit-examine-selfaware =
    • Вы носите { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    }.
outer-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на теле.
suitstorage-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на плечах.
back-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на спине.
gloves-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на руках.
belt-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на поясе.
shoes-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на стопах.

# Selfaware examine

comp-hands-examine-empty-selfaware = Вы ничего не держите.
comp-hands-examine-selfaware = Вы держите { $items }.
humanoid-appearance-component-examine-selfaware = Вы { INDEFINITE($age) } { $age } { $species }.
