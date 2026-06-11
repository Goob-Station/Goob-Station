comp-ssd-person-examined = [color=yellow]{ CAPITALIZE(SUBJECT($ent)) } has been asleep for { $time ->
    [one] { $time } minute
   *[other] { $time } minutes
}.[/color]
