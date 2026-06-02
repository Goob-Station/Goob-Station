comp-ssd-person-examined = [color=yellow]{ CAPITALIZE(SUBJECT($ent)) } dort depuis { $time ->
    [one] { $time } minute
   *[other] { $time } minutes
}.[/color]
