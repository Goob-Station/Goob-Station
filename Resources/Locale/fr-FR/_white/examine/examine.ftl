# Poggers examine system

examine-name = C'est [bold]{$name}[/bold] !
examine-name-selfaware = C'est vous !
examine-can-see = En regardant {OBJECT($ent)}, vous pouvez voir :
examine-can-see-nothing = {CAPITALIZE(SUBJECT($ent))} est complètement nu !
examine-border-line = ═════════════════════
examine-present-tex = C'est [enttex id="{ $id }" size={ $size }] [bold]{$name}[/bold]!
examine-present = C'est [bold]{$name}[/bold]!
examine-present-line = ═══

id-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} ceinture.
head-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} tête.
eyes-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} yeux.
mask-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} visage.
neck-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} necouck.
ears-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} oreilles.
jumpsuit-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} {SUBJECT($ent)} porte.
outer-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} son corps.
suitstorage-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} épaules.
back-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} dos.
gloves-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} mains.
belt-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} ceinture.
shoes-examine = • {CAPITALIZE(POSS-ADJ($ent))} { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur {POSS-ADJ($ent)} pieds.

id-card-examine-full = • {CAPITALIZE(POSS-ADJ($wearer))} ID: [bold]{$nameAndJob}[/bold].

# Selfaware version

examine-can-see-selfaware = En vous regardant, vous pouvez voir :
examine-can-see-nothing-selfaware = Vous êtes complètement nu !

id-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre ceinture.
head-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre tête.
eyes-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur vos yeux.
mask-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre visage.
neck-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre cou.
ears-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre oreilles.
jumpsuit-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} vous portez.
outer-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre corps.
suitstorage-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur vos épaules.
back-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre dos.
gloves-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur vos mains.
belt-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur votre ceinture.
shoes-examine-selfaware = • Votre { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} sur vos pieds.

# Selfaware examine

comp-hands-examine-empty-selfaware = Vous ne tenez rien dans les mains.
comp-hands-examine-selfaware = Vous tenez { $items }.

humanoid-appearance-component-examine-selfaware = Vous êtes { INDEFINITE($age) } { $age } { $species }.
