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