ore-silo-ui-title = Матеріальний силос
ore-silo-ui-label-clients = Машини
ore-silo-ui-label-mats = Матеріали
ore-silo-ui-itemlist-entry = {$linked ->
    [true] {"[Підключено] "}
    *[False] {""}
} {$name} ({$beacon}) {$inRange ->
    [true] {""}
    *[false] (поза радіусом)
}
ore-silo-ui-link-failed-unavailable = Машина більше недоступна для підключення.
ore-silo-ui-link-failed-unpowered = Силос не має живлення.
ore-silo-ui-link-failed-different-grid = Машина розташована на іншому гріді.
ore-silo-ui-link-failed-out-of-range = Машина поза радіусом дії силоса.