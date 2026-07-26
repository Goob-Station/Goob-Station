comp-ssd-person-examined = [color=yellow]{ CAPITALIZE(SUBJECT($ent)) } спит уже { $time ->
        [one] { $time } минуту
        [few] { $time } минуты
       *[other] { $time } минут
    }.[/color]
