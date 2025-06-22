-entity-heater-setting-name =
    { $setting ->
        [off] вимк
        [low] низький
        [medium] середній
        [high] високий
       *[other] невідомо
    }
entity-heater-examined = Встановлено на { $setting ->
    [off] [color=gray]{-entity-heater-setting-name(setting: "off")}[/color]
    [low] [color=yellow]{-entity-heater-setting-name(setting: "low")}[/color]
    [medium] [color=orange]{-entity-heater-setting-name(setting: "medium")}[/color]
    [high] [color=red]{-entity-heater-setting-name(setting: "high")}[/color]
   *[other] [color=purple]{-entity-heater-setting-name(setting: "other")}[/color]
}.
entity-heater-switch-setting = Перемкнути на { $setting ->
        [off] вимк
        [low] низький
        [medium] середній
        [high] високий
       *[other] невідомо
    }
entity-heater-switched-setting = Перемкнено на { $setting ->
        [off] вимк
        [low] низький
        [medium] середній
        [high] високий
       *[other] невідомо
    }.
