guidebook-reagent-effect-description = { $chance ->
        [1] { $effect }
        *[other] Має { NATURALPERCENT($chance, 2) } шанс на { $effect }
    }{ $conditionCount ->
        [0] .
        *[other] {" "}коли { $conditions }.
    }

guidebook-reagent-name = [bold][color={$color}]{CAPITALIZE($name)}[/color][/bold]
guidebook-reagent-recipes-header = Рецепт
guidebook-reagent-recipes-reagent-display = [bold]{$reagent}[/bold] \[{$ratio}\]
guidebook-reagent-sources-header = Джерела
guidebook-reagent-sources-ent-wrapper = [bold]{$name}[/bold] \[1\]
guidebook-reagent-sources-gas-wrapper = [bold]{$name} (газ)[/bold] \[1\]
guidebook-reagent-effects-header = Ефекти
guidebook-reagent-effects-metabolism-group-rate = [bold]{$group}[/bold] [color=gray]({$rate} одиниць на секунду)[/color]
guidebook-reagent-physical-description = [italic]Схоже на {$description} речовину.[/italic]
guidebook-reagent-recipes-mix-info = {$minTemp ->
    [0] {$hasMax ->
            [true] {CAPITALIZE($verb)} нижче {NATURALFIXED($maxTemp, 2)}K
            *[false] {CAPITALIZE($verb)}
        }
    *[other] {CAPITALIZE($verb)} {$hasMax ->
            [true] між {NATURALFIXED($minTemp, 2)}K та {NATURALFIXED($maxTemp, 2)}K
            *[false] вище {NATURALFIXED($minTemp, 2)}K
        }
}
