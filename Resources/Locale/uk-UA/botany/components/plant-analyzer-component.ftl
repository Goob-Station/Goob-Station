plant-analyzer-component-no-seed = рослина не знайдена

plant-analyzer-component-health = Здоров'я:
plant-analyzer-component-age = Вік:
plant-analyzer-component-water = Вода:
plant-analyzer-component-nutrition = Поживність:
plant-analyzer-component-toxins = Токсини:
plant-analyzer-component-pests = Шкідники:
plant-analyzer-component-weeds = Бур'яни:

plant-analyzer-component-alive = [color=green]ЖИВИЙ[/color]
plant-analyzer-component-dead = [color=red]МЕРТВИЙ[/color]
plant-analyzer-component-unviable = [color=red]НЕЖИТТЄЗДАТНИЙ[/color]
plant-analyzer-component-mutating = [color=#00ff5f]МУТУЄ[/color]
plant-analyzer-component-kudzu = [color=red]КУДЗУ[/color]

plant-analyzer-soil = У цьому {$holder} є трохи [color=white]{$chemicals}[/color], що {$count ->
    [one]не була
    *[other]не були
} поглинені.
plant-analyzer-soil-empty = У цьому {$holder} немає непоглинених хімікатів.

plant-analyzer-component-environment = Ця [color=green]{$seedName}[/color] потребує атмосферу з тиском [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color], температурою [color=lightsalmon]{$temp}°k ± {$tempTolerance}°k[/color] та рівнем освітлення [color=white]{$lightLevel} ± {$lightTolerance}[/color].
plant-analyzer-component-environment-void = Цю [color=green]{$seedName}[/color] потрібно вирощувати [bolditalic]у космічному вакуумі[/bolditalic] при рівні освітлення [color=white]{$lightLevel} ± {$lightTolerance}[/color].
plant-analyzer-component-environment-gas = Ця [color=green]{$seedName}[/color] потребує атмосферу, що містить [bold]{$gases}[/bold] з тиском [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color], температурою [color=lightsalmon]{$temp}°k ± {$tempTolerance}°k[/color] та рівнем освітлення [color=white]{$lightLevel} ± {$lightTolerance}[/color].

plant-analyzer-produce-plural = {MAKEPLURAL($thing)}
plant-analyzer-output = {$yield ->
    [0]{$gasCount ->
        [0]Здається, єдине, що вона робить, це споживає воду та поживні речовини.
        *[other]Здається, єдине, що вона робить, це перетворює воду та поживні речовини на [bold]{$gases}[/bold].
    }
    *[other]Вона має [color=lightgreen]{$yield} {$potency}[/color]{$seedless ->
        [true]{" "}але [color=red]без насіння[/color]
        *[false]{$nothing}
    }{" "}{$yield ->
        [one]квітку
        *[other]квітки
    }{" "}що{$gasCount ->
        [0]{$nothing}
        *[other]{$yield ->
            [one]{" "}випромінює
            *[other]{" "}випромінюють
        }{" "}[bold]{$gases}[/bold] і
    }{" "}перетвориться на{$yield ->
        [one]{" "}{INDEFINITE($firstProduce)} [color=#a4885c]{$produce}[/color]
        *[other]{" "}[color=#a4885c]{$producePlural}[/color]
    }.{$chemCount ->
        [0]{$nothing}
        *[other]{" "}У її стеблі є сліди [color=white]{$chemicals}[/color].
    }
}

plant-analyzer-potency-tiny = крихітний
plant-analyzer-potency-small = малий
plant-analyzer-potency-below-average = розміром нижче середнього
plant-analyzer-potency-average = середнього розміру
plant-analyzer-potency-above-average = розміром вище середнього
plant-analyzer-potency-large = досить великий
plant-analyzer-potency-huge = величезний
plant-analyzer-potency-gigantic = гігантський
plant-analyzer-potency-ludicrous = сміховинно великий
plant-analyzer-potency-immeasurable = невимірно великий

plant-analyzer-print = Друк
plant-analyzer-printout-missing = Н/Д
plant-analyzer-printout = [color=#9FED58][head=2]Звіт аналізатора рослин[/head][/color]{$nl
    }──────────────────────────────{$nl
    }[bullet/] Вид: {$seedName}{$nl
    }{$indent}[bullet/] Життєздатність: {$viable ->
        [no][color=red]Ні[/color]
        [yes][color=green]Так[/color]
        *[other]{LOC("plant-analyzer-printout-missing")}
    }{$nl
    }{$indent}[bullet/] Витривалість: {$endurance}{$nl
    }{$indent}[bullet/] Тривалість життя: {$lifespan}{$nl
    }{$indent}[bullet/] Продукт: [color=#a4885c]{$produce}[/color]{$nl
    }{$indent}[bullet/] Кудзу: {$kudzu ->
        [no][color=green]Ні[/color]
        [yes][color=red]Так[/color]
        *[other]{LOC("plant-analyzer-printout-missing")}
    }{$nl
    }[bullet/] Профіль росту:{$nl
    }{$indent}[bullet/] Вода: [color=cyan]{$water}[/color]{$nl
    }{$indent}[bullet/] Поживність: [color=orange]{$nutrients}[/color]{$nl
    }{$indent}[bullet/] Токсини: [color=yellowgreen]{$toxins}[/color]{$nl
    }{$indent}[bullet/] Шкідники: [color=magenta]{$pests}[/color]{$nl
    }{$indent}[bullet/] Бур'яни: [color=red]{$weeds}[/color]{$nl
    }[bullet/] Екологічний профіль:{$nl
    }{$indent}[bullet/] Склад: [bold]{$gasesIn}[/bold]{$nl
    }{$indent}[bullet/] Тиск: [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color]{$nl
    }{$indent}[bullet/] Температура: [color=lightsalmon]{$temp}°k ± {$tempTolerance}°k[/color]{$nl
    }{$indent}[bullet/] Освітлення: [color=gray][bold]{$lightLevel} ± {$lightTolerance}[/bold][/color]{$nl
    }[bullet/] Квіти: {$yield ->
        [-1]{LOC("plant-analyzer-printout-missing")}
        [0][color=red]0[/color]
        *[other][color=lightgreen]{$yield} {$potency}[/color]
    }{$nl
    }[bullet/] Насіння: {$seeds ->
        [no][color=red]Ні[/color]
        [yes][color=green]Так[/color]
        *[other]{LOC("plant-analyzer-printout-missing")}
    }{$nl
    }[bullet/] Хімікати: [color=gray][bold]{$chemicals}[/bold][/color]{$nl
    }[bullet/] Викиди: [bold]{$gasesOut}[/bold]

plant-analyzer-component-environemt = Цьому [color=green]{$seedName}[/color] потрібна атмосфера з тиском [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color], температурою [color=lightsalmon]{$temp}°k ± {$tempTolerance}°k[/color] та рівнем освітлення [color=white]{$lightLevel} ± {$lightTolerance}[/color].
plant-analyzer-component-environemt-void = Це [color=green]{$seedName}[/color] має вирощуватися [bolditalic]у вакуумі космосу[/bolditalic] при рівні освітлення [color=white]{$lightLevel} ± {$lightTolerance}[/color].
plant-analyzer-component-environemt-gas = Цьому [color=green]{$seedName}[/color] потрібна атмосфера, що містить [bold]{$gases}[/bold], з тиском [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color], температурою [color=lightsalmon]{$temp}°k ± {$tempTolerance}°k[/color] та рівнем освітлення [color=white]{$lightLevel} ± {$lightTolerance}[/color].