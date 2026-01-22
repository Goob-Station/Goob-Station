# Poggers examine system

examine-name = Это [bold]{ $name }[/bold]!
examine-name-selfaware = Это вы!
examine-can-see = Смотря на { OBJECT($ent) }, вы можете видеть:
examine-can-see-nothing = { CAPITALIZE(SUBJECT($ent)) } полностью без одежды!
examine-border-line = ═════════════════════
examine-present-tex = Это [enttex id="{ $id }" size={ $size }] [bold]{ $name }[/bold]!
examine-present = Это [bold]{ $name }[/bold]!
examine-present-line = ═══
id-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } поясе.
head-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } голове.
eyes-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } глазах.
mask-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } лице.
neck-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } шее.
ears-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } ухе.
jumpsuit-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } теле.
outer-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } теле.
suitstorage-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } плече.
back-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } спине.
gloves-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } руках.
belt-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } пояснице.
shoes-examine =
    • { CAPITALIZE(POSS-ADJ($ent)) } { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на { POSS-ADJ($ent) } ногах.
id-card-examine-full = • { CAPITALIZE(POSS-ADJ($wearer)) } ID: [bold]{ $nameAndJob }[/bold].

# Selfaware version

examine-can-see-selfaware = Смотря на себя, вы можете увидеть:
examine-can-see-nothing-selfaware = Вы абсолютно голые!
id-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашем поясе.
head-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашей голове.
eyes-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на ваших глазах.
mask-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашем лице.
neck-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашей шее.
ears-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашем ухе.
jumpsuit-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашем теле.
outer-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашем теле.
suitstorage-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашем плече.
back-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашей спине.
gloves-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на ваших руках.
belt-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на вашей пояснице.
shoes-examine-selfaware =
    • { $id ->
        [empty] [bold]{ $item }[/bold]
       *[other] [enttex id="{ $id }" size={ $size }][bold]{ $item }[/bold]
    } на ваших ногах.

# Selfaware examine

comp-hands-examine-empty-selfaware = Вы ничего не держите.
comp-hands-examine-selfaware = Вы держите { $items }.
humanoid-appearance-component-examine-selfaware = Вы { $species } { $age }.
