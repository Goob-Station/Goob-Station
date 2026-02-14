shared-solution-container-component-on-examine-empty-container = Не содержит вещества.
shared-solution-container-component-on-examine-main-text = Содержит [color={ $color }]{ $desc }[/color] { $wordedAmount }
shared-solution-container-component-on-examine-worded-amount-one-reagent = вещество.
examinable-solution-recognized = [color={ $color }]{ $chemical }[/color]
examinable-solution-on-examine-volume = Ёмкость { $fillLevel ->
    [exact] содержит [color=white]{$current}/{$max}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-no-max = Содержимое раствора { $fillLevel ->
    [exact] содержит [color=white]{$current}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-puddle =
    Лужа { $fillLevel ->
        [exact] содержит [color=white]{ $current }u[/color].
        [full] огромная и льётся через край!
        [mostlyfull] огромная и льётся через край!
        [halffull] глубокая и растекается.
        [halfempty] очень глубокая.
       *[mostlyempty] скапливается в лужицы.
        [empty] образует несколько маленьких луж.
    }
solution-vague-fill-level =
    { $fillLevel ->
        [full] [color=white]заполнена[/color]
        [mostlyfull] [color=#DFDFDF]почти заполнена[/color]
        [halffull] [color=#C8C8C8]наполовину полная[/color]
        [halfempty] [color=#C8C8C8]наполовину пустая[/color]
        [mostlyempty] [color=#A4A4A4]почти пустая[/color]
       *[empty] [color=gray]пустая[/color]
    }
shared-solution-container-component-on-examine-worded-amount-multiple-reagents = смесь веществ.
examinable-solution-has-recognizable-chemicals = В этом растворе вы можете распознать { $recognizedString }.
examinable-solution-recognized-first = [color={ $color }]{ $chemical }[/color]
examinable-solution-recognized-next = , [color={ $color }]{ $chemical }[/color]
examinable-solution-recognized-last = и [color={ $color }]{ $chemical }[/color]
