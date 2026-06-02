# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

shared-solution-container-component-on-examine-main-text = Il contient { $chemCount ->
    [1] [color={$color}]{$desc}[/color].
   *[other] un mélange [color={$color}]{$desc}[/color].
    }
examinable-solution-has-recognizable-chemicals = Vous pouvez reconnaître {$recognizedString} dans la solution.
examinable-solution-recognized = [color={$color}]{$chemical}[/color]
examinable-solution-on-examine-volume = La solution contenue { $fillLevel ->
    [exact] contient [color=white]{$current}/{$max}u[/color].
   *[other] est [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}
examinable-solution-on-examine-volume-no-max = La solution contenue { $fillLevel ->
    [exact] contient [color=white]{$current}u[/color].
   *[other] est [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}
examinable-solution-on-examine-volume-puddle = La flaque { $fillLevel ->
    [exact] fait [color=white]{$current}u[/color].
    [full] est immense et déborde !
    [mostlyfull] est immense et déborde !
    [halffull] est profonde et s'écoule.
    [halfempty] est très profonde.
   *[mostlyempty] se rassemble en flaques.
    [empty] forme plusieurs petites mares.
}

-solution-vague-fill-level =
    { $fillLevel ->
        [full] [color=white]Plein[/color]
        [mostlyfull] [color=#DFDFDF]Presque plein[/color]
        [halffull] [color=#C8C8C8]À moitié plein[/color]
        [halfempty] [color=#C8C8C8]À moitié vide[/color]
        [mostlyempty] [color=#A4A4A4]Presque vide[/color]
       *[empty] [color=gray]Vide[/color]
    }
