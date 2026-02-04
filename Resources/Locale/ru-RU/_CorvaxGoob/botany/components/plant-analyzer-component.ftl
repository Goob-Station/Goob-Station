plant-analyzer-component-no-seed = растение не обнаружено

plant-analyzer-and = и

plant-analyzer-component-health = Здоровье:
plant-analyzer-component-age = Возраст:
plant-analyzer-component-water = Вода:
plant-analyzer-component-nutrition = Питание:
plant-analyzer-component-toxins = Токсины:
plant-analyzer-component-pests = Вредители:
plant-analyzer-component-weeds = Сорняки:

plant-analyzer-component-alive = [color=green]ЖИВОЕ[/color]
plant-analyzer-component-dead = [color=red]МЁРТВОЕ[/color]
plant-analyzer-component-unviable = [color=red]ПОГИБАЕТ[/color]
plant-analyzer-component-mutating = [color=#00ff5f]МУТИРУЕТ[/color]
plant-analyzer-component-kudzu = [color=red]КУДЗУ[/color]

plant-analyzer-soil = В {$holder} находится [color=white]{$chemicals}[/color], {$count ->
    [one]который впитывается в растение
    *[other]которые впитываются в растение
}.
plant-analyzer-soil-empty = В {$holder} сейчас ничего не впитывается из химикатов.

plant-analyzer-component-environemt = Растение [color=green]{$seedName}[/color] требует атмосферу с уровнем давления [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color], температуру [color=lightsalmon]{$temp}°К ± {$tempTolerance}°К[/color] и уровень освещения [color=white]{$lightLevel} ± {$lightTolerance}[/color].
plant-analyzer-component-environemt-void = Растение [color=green]{$seedName}[/color] должно выращиваться [bolditalic]в вакууме космоса[/bolditalic] при уровне освещения [color=white]{$lightLevel} ± {$lightTolerance}[/color].
plant-analyzer-component-environemt-gas = Растение [color=green]{$seedName}[/color] требует атмосферу, содержащую [bold]{$gases}[/bold], с уровнем давления [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color], температуру [color=lightsalmon]{$temp}°К ± {$tempTolerance}°К[/color] и уровень освещения [color=white]{$lightLevel} ± {$lightTolerance}[/color].

plant-analyzer-produce-plural = {$thing}
plant-analyzer-output = {$yield ->
    [0]{$gasCount ->
        [0]Единственное, что оно делает - потребляет воду и питательные вещества.
        *[other]Единственное, что оно делает - превращает воду и питательные вещества в [bold]{$gases}[/bold].
    }
    *[other]Имеет [color=lightgreen]{$yield} {$potency}[/color]{$seedless ->
        [true]{" "}но [color=red]бессемянных[/color]
        *[false]{$nothing}
    }{" "}{$yield ->
        [one]цветка
        [2]цветка
        [3]цветка
        [4]цветка
        *[other]цветков
    }, {$yield ->
        [one]который
        *[other]которые
    }{$gasCount ->
        [0]{$nothing}
        *[other]{" "}{$yield ->
            [one]выделяет
            *[other]выделяют
        }{" "}[bold]{$gases}[/bold] и
    }{" "}{$yield ->
        [one]превратится
        *[other]превратятся
    } в [color=#a4885c]{$producePlural}[/color].{$chemCount ->
        [0]{$nothing}
        *[other]{" "}В стебле присутствуют следы [color=white]{$chemicals}[/color].
    }
}

plant-analyzer-potency-tiny = крошечных
plant-analyzer-potency-small = маленьких
plant-analyzer-potency-below-average = ниже среднего размера
plant-analyzer-potency-average = среднего размера
plant-analyzer-potency-above-average = выше среднего размера
plant-analyzer-potency-large = довольно крупных
plant-analyzer-potency-huge = огромных
plant-analyzer-potency-gigantic = гигантских
plant-analyzer-potency-ludicrous = невероятно больших
plant-analyzer-potency-immeasurable = неизмеримо больших

plant-analyzer-print = Распечатать отчёт
plant-analyzer-printout-missing = Н/Д
plant-analyzer-printout =
    {"[color=#9FED58][head=2]Отчёт анализатора растений[/head][/color]"}
    ──────────────────────────────
    {"[bullet/]"} Вид: {$seedName}
    {"    "}[bullet/] Жизнеспособность: {$viable ->
        [no][color=red]Нет[/color]
        [yes][color=green]Да[/color]
        *[other]{LOC("plant-analyzer-printout-missing")}
    }
    {"    "}[bullet/] Выносливость: {$endurance}
    {"    "}[bullet/] Продолжительность жизни: {$lifespan}
    {"    "}[bullet/] Продукция: [color=#a4885c]{$produce}[/color]
    {"    "}[bullet/] Кудзу: {$kudzu ->
        [no][color=green]Нет[/color]
        [yes][color=red]Да[/color]
        *[other]{LOC("plant-analyzer-printout-missing")}
    }
    {"[bullet/]"} Профиль роста:
    {"    "}[bullet/] Вода: [color=cyan]{$water}[/color]
    {"    "}[bullet/] Питание: [color=orange]{$nutrients}[/color]
    {"    "}[bullet/] Токсины: [color=yellowgreen]{$toxins}[/color]
    {"    "}[bullet/] Вредители: [color=magenta]{$pests}[/color]
    {"    "}[bullet/] Сорняки: [color=red]{$weeds}[/color]
    {"[bullet/]"} Профиль окружающей среды:
    {"    "}[bullet/] Состав: [bold]{$gasesIn}[/bold]
    {"    "}[bullet/] Давление: [color=lightblue]{$kpa}кПа ± {$kpaTolerance}кПа[/color]
    {"    "}[bullet/] Температура: [color=lightsalmon]{$temp}°К ± {$tempTolerance}°К[/color]
    {"    "}[bullet/] Освещение: [color=gray][bold]{$lightLevel} ± {$lightTolerance}[/bold][/color]
    {"[bullet/]"} Цветы: {$yield ->
        [-1]{LOC("plant-analyzer-printout-missing")}
        [0][color=red]0[/color]
        *[other][color=lightgreen]{$yield} {$potency}[/color]
    }
    {"[bullet/]"} Семена: {$seeds ->
        [no][color=red]Нет[/color]
        [yes][color=green]Да[/color]
        *[other]{LOC("plant-analyzer-printout-missing")}
    }
    {"[bullet/]"} Химикаты: [color=gray][bold]{$chemicals}[/bold][/color]
    {"[bullet/]"} Выбросы: [bold]{$gasesOut}[/bold]
