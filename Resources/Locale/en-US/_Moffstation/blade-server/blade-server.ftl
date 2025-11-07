moff-blade-server-rack-window-title = Blade Server Rack
moff-blade-server-rack-window-footer-flavor = DEVICE FIRMWARE © 2125 NANOSOFT

moff-blade-server-rack-slot-status = Slot {$index}: {$content}

moff-blade-server-rack-slot-entity-unknown = unknown
moff-blade-server-rack-slot-empty = vacant

moff-blade-server-rack-slot-eject = Eject
moff-blade-server-rack-slot-insert = Insert
moff-blade-server-rack-slot-power-toggle = Toggle Power

moff-blade-server-rack-slot-locked-fail = It's locked!
moff-blade-server-rack-slot-whitelist-fail = That doesn't fit!

moff-blade-server-rack-examine-empty = It contains [color=#1f8ab2]no blades[/color].
moff-blade-server-rack-examine-single = It contains only {$slot}.
moff-blade-server-rack-examine-multiple-start = It contains
moff-blade-server-rack-examine-multiple-slot-line = - {$slot}
moff-blade-server-rack-examine-slot = { INDEFINITE($name) } [color=#1f8ab2]{ CAPITALIZE($name) }[/color] in slot {$index}
moff-blade-server-rack-examine-distant =
    It contains [color=#1f8ab2]{$numBlades} { $numBlades ->
        [1] blade
        *[other] blades
    }[/color], but you can't tell what { $numBlades ->
        [1] it is
        *[other] they are
    } from this distance.

moff-blade-server-frame-incompatible-board = This board seems incompatible with the frame...
moff-blade-server-board-compatible-hint = It can be used to make a [color=#1f8ab2]blade server[/color]
