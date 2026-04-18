-entity-heater-setting-name =
    { $setting ->
        [off] off
        [low] low
        [medium] medium
        [high] high
       *[other] unknown
    }

entity-heater-examined =
    Выбран режим { $setting ->
        [off] [color=gray]{ -entity-heater-setting-name(setting: "off") }[/color]
        [low] [color=yellow]{ -entity-heater-setting-name(setting: "low") }[/color]
        [medium] [color=orange]{ -entity-heater-setting-name(setting: "medium") }[/color]
        [high] [color=red]{ -entity-heater-setting-name(setting: "high") }[/color]
       *[other] [color=purple]{ -entity-heater-setting-name(setting: "other") }[/color]
    }.
entity-heater-switch-setting = Переключить на { -entity-heater-setting-name(setting: $setting) }
entity-heater-switched-setting = Переключён на { -entity-heater-setting-name(setting: $setting) }.
