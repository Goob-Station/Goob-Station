plant-analyzer-component-no-seed = растение не найдено
plant-analyzer-component-health = Здоровье:
plant-analyzer-component-age = Возраст:
plant-analyzer-component-water = Вода:
plant-analyzer-component-nutrition = Пит.вещ:
plant-analyzer-component-toxins = Токсины:
plant-analyzer-component-pests = Вредители:
plant-analyzer-component-weeds = Сорняки:
plant-analyzer-component-alive = [color=green]ЖИВОЕ[color]
plant-analyzer-component-dead = [color=red]МЕРТВОЕ[color]
plant-analyzer-component-unviable = [color=red]ГЕН СМЕРТИ[color]
plant-analyzer-component-mutating = [color=#00ff5f]МУТИРУЕТ[color]
plant-analyzer-component-kudzu = [color=red]КУДЗУ[color]
plant-analyzer-soil =
    В этом { $holder } содержится некоторое количество [color=white]{ $chemicals }[/color], которое { $count ->
        [one] имеет
       *[other] имеют
    } ещё не поглощено.
plant-analyzer-soil-empty = В этом { $holder } нет непоглощенных химических веществ.
plant-analyzer-component-environemt = Это [color=green]{ $seedName }[/color] требует атмосферы при уровне давления [color=lightblue]{ $kpa }кПа ± { $kpaTolerance }кПа[/color], температуры [color=lightsalmon]{ $temp }°к ± { $tempTolerance }°к[/color] и уровня освещения [color=white]{ $lightLevel } ± { $lightTolerance }[/color].
plant-analyzer-component-environemt-void = Это [color=green]{ $seedName }[/color] должно выращиваться [bolditalic]в вакууме космоса[/bolditalic] при уровне освещения [color=white]{ $lightLevel } ± { $lightTolerance }[/color].
plant-analyzer-component-environemt-gas = Это [color=green]{ $seedName }[/color] требует атмосферы, содержащей [bold]{ $gases }[/bold] при уровне давления [color=lightblue]{ $kpa }кПа ± { $kpaTolerance }кПа[/color], температуры [color=lightsalmon]{ $temp }°к ± { $tempTolerance }°к[/color] и уровне освещения [color=white]{ $lightLevel } ± { $lightTolerance }[/color].
plant-analyzer-produce-plural = { $thing }
plant-analyzer-output =
    { $yield ->
        [0]
            { $gasCount ->
                [0] Единственное, что оно, похоже, делает, это потребляет воду и питательные вещества.
               *[other] Единственное, что оно, похоже, делает, это превращает воду и питательные вещества в [bold]{ $gases }[/bold].
            }
       *[other]
            Оно имеет [color=lightgreen]{ $yield } { $potency }[/color]{ $seedless ->
                [true] { " " }но [color=red]без семян[/color]
               *[false] { $nothing }
            }{ " " }{ $yield ->
                [one] цветок
               *[other] цветков
            }{ " " }который{ $gasCount ->
                [0] { $nothing }
               *[other]
                    { $yield ->
                        [one] { " " }выделяет
                       *[other] { " " }выделяют
                    }{ " " }[bold]{ $gases }[/bold] и
            }{ " " }превратится в{ $yield ->
                [one] { " " }{ INDEFINITE($firstProduce) } [color=#a4885c]{ $produce }[/color]
               *[other] { " " }[color=#a4885c]{ $producePlural }[/color]
            }.{ $chemCount ->
                [0] { $nothing }
               *[other] { " " }В его стебле обнаружены следовые количества [color=white]{ $chemicals }[/color].
            }
    }
plant-analyzer-potency-tiny = микроскопическое
plant-analyzer-potency-small = маленькое
plant-analyzer-potency-below-average = ниже среднего размера
plant-analyzer-potency-average = среднего размера
plant-analyzer-potency-above-average = выше среднего размера
plant-analyzer-potency-large = довольно большое
plant-analyzer-potency-huge = огромное
plant-analyzer-potency-gigantic = гигантское
plant-analyzer-potency-ludicrous = нелепо большое
plant-analyzer-potency-immeasurable = немерено большое
plant-analyzer-print = Печать
plant-analyzer-printout-missing = Н/Д
plant-analyzer-printout = [color=#9FED58][head=2]Отчет анализатора растений[/head][/color]{ $nl }──────────────────────────────{ $nl }[bullet/] Вид: { $seedName }{ $nl }{ $indent }[bullet/] Пригодность: { $viable ->
        [no] [color=red]Нет[/color]
        [yes] [color=green]Да[/color]
       *[other] { LOC("plant-analyzer-printout-missing") }
    }{ $nl }{ $indent }[bullet/] Выносливость: { $endurance }{ $nl }{ $indent }[bullet/] Продолжительность жизни: { $lifespan }{ $nl }{ $indent }[bullet/] Продукт: [color=#a4885c]{ $produce }[/color]{ $nl }{ $indent }[bullet/] Кудзу: { $kudzu ->
        [no] [color=green]Нет[/color]
        [yes] [color=red]Да[/color]
       *[other] { LOC("plant-analyzer-printout-missing") }
    }{ $nl }[bullet/] Профиль роста:{ $nl }{ $indent }[bullet/] Вода: [color=cyan]{ $water }[/color]{ $nl }{ $indent }[bullet/] Питательные вещества: [color=orange]{ $nutrients }[/color]{ $nl }{ $indent }[bullet/] Токсины: [color=yellowgreen]{ $toxins }[/color]{ $nl }{ $indent }[bullet/] Вредители: [color=magenta]{ $pests }[/color]{ $nl }{ $indent }[bullet/] Сорняки: [color=red]{ $weeds }[/color]{ $nl }[bullet/] Профиль окружающей среды:{ $nl }{ $indent }[bullet/] Состав: [bold]{ $gasesIn }[/bold]{ $nl }{ $indent }[bullet/] Давление: [color=lightblue]{ $kpa }kPa ± { $kpaTolerance }kPa[/color]{ $nl }{ $indent }[bullet/] Температура: [color=lightsalmon]{ $temp }°k ± { $tempTolerance }°k[/color]{ $nl }{ $indent }[bullet/] Освещение: [color=gray][bold]{ $lightLevel } ± { $lightTolerance }[/bold][/color]{ $nl }[bullet/] Цветы: { $yield ->
        [-1] { LOC("plant-analyzer-printout-missing") }
        [0] [color=red]0[/color]
       *[other] [color=lightgreen]{ $yield } { $potency }[/color]
    }{ $nl }[bullet/] Семена: { $seeds ->
        [no] [color=red]Нет[/color]
        [yes] [color=green]Да[/color]
       *[other] { LOC("plant-analyzer-printout-missing") }
    }{ $nl }[bullet/] Химические вещества: [color=gray][bold]{ $chemicals }[/bold][/color]{ $nl }[bullet/] Выбросы: [bold]{ $gasesOut }[/bold]


plant-analyzer-yes = да
plant-analyzer-no = нет
plant-analyzer-unknown = неизвестно

plant-analyzer-potency-low = низкая
plant-analyzer-potency-medium = средняя
plant-analyzer-potency-high = высокая
