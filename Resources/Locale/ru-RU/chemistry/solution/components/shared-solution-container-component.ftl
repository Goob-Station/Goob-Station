# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

shared-solution-container-component-on-examine-main-text =
    Содержит { INDEFINITE($desc) } [color={ $color }]{ $desc }[/color] { $chemCount ->
        [1] вещество.
       *[other] смесь химических веществ.
    }

examinable-solution-has-recognizable-chemicals = В этом растворе вы можете распознать { $recognizedString }.
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

-solution-vague-fill-level =
    { $fillLevel ->
        [full] [color=white]заполнена[/color]
        [mostlyfull] [color=#DFDFDF]почти заполнена[/color]
        [halffull] [color=#C8C8C8]наполовину полная[/color]
        [halfempty] [color=#C8C8C8]наполовину пустая[/color]
        [mostlyempty] [color=#A4A4A4]почти пустая[/color]
       *[empty] [color=gray]пустая[/color]
    }
