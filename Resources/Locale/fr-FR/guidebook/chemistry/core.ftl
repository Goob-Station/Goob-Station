# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 TomaszKawalec <40093912+TK-A369@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Flesh <62557990+PolterTzi@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

guidebook-reagent-effect-description =
    {$chance ->
        [1] { $effect }
        *[other] A { NATURALPERCENT($chance, 2) } de chances de { $effect }
    }{ $conditionCount ->
        [0] .
        *[other] {" "}lorsque { $conditions }.
    }
    
guidebook-reagent-name = [bold][color={$color}]{CAPITALIZE($name)}[/color][/bold]
guidebook-reagent-recipes-header = Recette
guidebook-reagent-recipes-reagent-display = [bold]{$reagent}[/bold] \[{$ratio}\]
guidebook-reagent-sources-header = Sources
guidebook-reagent-sources-ent-wrapper = [bold]{$name}[/bold] \[1\]
guidebook-reagent-sources-gas-wrapper = [bold]{$name} (gaz)[/bold] \[1\]
guidebook-reagent-effects-header = Effets
guidebook-reagent-effects-metabolism-group-rate = [bold]{$group}[/bold] [color=gray]({$rate} unités par seconde)[/color]
guidebook-reagent-plant-metabolisms-header = Métabolisme végétal
guidebook-reagent-plant-metabolisms-rate = [bold]Métabolisme végétal[/bold] [color=gray](1 unité toutes les 3 secondes en base)[/color]
guidebook-reagent-physical-description = [italic]Semble être {$description}.[/italic]
guidebook-reagent-recipes-mix-info = {$minTemp ->
    [0] {$hasMax ->
            [true] {CAPITALIZE($verb)} en dessous de {NATURALFIXED($maxTemp, 2)}K
            *[false] {CAPITALIZE($verb)}
        }
    *[other] {CAPITALIZE($verb)} {$hasMax ->
            [true] entre {NATURALFIXED($minTemp, 2)}K et {NATURALFIXED($maxTemp, 2)}K
            *[false] au-dessus de {NATURALFIXED($minTemp, 2)}K
        }
}
