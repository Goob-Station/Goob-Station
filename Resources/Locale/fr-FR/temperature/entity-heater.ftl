-entity-heater-setting-name =
    { $setting ->
        [off] éteint
        [low] bas
        [medium] moyen
        [high] élevé
       *[other] inconnu
    }

entity-heater-examined = Il est réglé sur { $setting ->
    [off] [color=gray]{ -entity-heater-setting-name(setting: "off") }[/color]
    [low] [color=yellow]{ -entity-heater-setting-name(setting: "low") }[/color]
    [medium] [color=orange]{ -entity-heater-setting-name(setting: "medium") }[/color]
    [high] [color=red]{ -entity-heater-setting-name(setting: "high") }[/color]
   *[other] [color=purple]{ -entity-heater-setting-name(setting: "other") }[/color]
}.
entity-heater-switch-setting = Régler sur { -entity-heater-setting-name(setting: $setting) }
entity-heater-switched-setting = Réglé sur { -entity-heater-setting-name(setting: $setting) }.
