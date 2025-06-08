### Locale for wielding items; i.e. two-handing them

wieldable-verb-text-wield = Схопити
wieldable-verb-text-unwield = Відпустити

wieldable-component-successful-wield = Ви схопили { THE($item) } в дві руки.
wieldable-component-failed-wield = Ви переклали { THE($item) } в одну руку.
wieldable-component-successful-wield-other = { THE($user) } схопив { THE($item) }.
wieldable-component-failed-wield-other = { THE($user) } переклав { THE($item) } в одну руку.

wieldable-component-no-hands = У вас нема вільних рук!
wieldable-component-not-enough-free-hands = {$number ->
    [one] Вам потрібна вільна рука щоб схопити { THE($item) }.
    *[other] Вам треба { $number } вільних рук щоб схопити { THE($item) }.
}
wieldable-component-not-in-hands = { CAPITALIZE(THE($item)) } не у ваших руках!

wieldable-component-requires = { CAPITALIZE(THE($item))} має бути в двох руках!


gunwieldbonus-component-examine = Ця зброя має підвищену точність стрільби.
gunrequireswield-component-examine = З цієї зброї можна стріляти лише тоді, коли вона в руках.