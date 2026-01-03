# templates
# service
ntr-document-service-starting-text1 = [color=#009100]█▄ █ ▀█▀    [head=3]Документ NanoTrasen[/head]
    █ ▀█     █        Для: Сервисного отдела
                           От: Центрального командования
                           Издан: {$date}
    ──────────────────────────────────────────[/color]

# security
ntr-document-security-starting-text1 = [head=3]Документ NanoTrasen[/head]                               [color=#990909]█▄ █ ▀█▀
    Для: Отдела Службы безопасности              █ ▀█     █
    От: Центрального командования
    Издан: {$date}
    ──────────────────────────────────────────[/color]

# cargo
ntr-document-cargo-starting-text1 = [head=3]Документ NanoTrasen[/head]                         [color=#d48311]        █▄ █ ▀█▀ [bold]
    Для: Отдел Снабжения[/bold][head=3][/head]               [color=#d48311]                         █ ▀█     █        [bold]
    От: Центральное командование[/bold]
    Издан: {$date}
    ──────────────────────────────────────────[/color]

# medical
ntr-document-medical-starting-text1 = [color=#118fd4]░            █▄ █ ▀█▀    [head=3]Документ NanoTrasen[/head]                       ░
    █            █ ▀█     █        Для: Медицинского отдела                         █
    ░                 От: Центрального командования                              ░
                        Издан: {$date}
    ──────────────────────────────────────────[/color]

# engineering
ntr-document-engineering-starting-text1 = [color=#a15000]█▄ █ ▀█▀    [head=3]Документ NanoTrasen[/head]
    █ ▀█     █        Для: Инженерного отдела
                       От: Центрального командования
                       Издан: {$date}
    ──────────────────────────────────────────[/color]
# science
ntr-document-science-starting-text1 = [color=#94196f]░             █▄ █ ▀█▀    [head=3]Документ NanoTrasen[/head]                 ░
    █             █ ▀█     █        Для: Научного отдела                             █
    ░                                    От: Центрального командования      ░
                                         Издан: {$date}
    ──────────────────────────────────────────[/color]
ntr-document-service-document-text =
    {$start}
    Корпорация сообщает, что вы не являетесь {$text1} {$text2}
    Корпорация будет рада, если вы {$text3}
    Штампы ниже подтверждают, что {$text4}

ntr-document-security-document-text =
    {$start}
    Корпорация требует проверить кое-что перед подписью этого документа: убедитесь, что {$text1} {$text2}
    {$text3}
    {$text4}

ntr-document-cargo-document-text =
    {$start}
    {$text1}
    {$text2}
    Ставя здесь печать, вы {$text3}

ntr-document-medical-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    Ставя здесь печать, вы {$text4}

ntr-document-engineering-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    Ставя здесь печать, вы {$text4}

ntr-document-science-document-text =
    {$start}
    Мы пристально следим за Научным отделом. {$text1} {$text2}
    В связи со всем вышеизложенным, мы требуем от вас гарантий, что {$text3}
    Штампы ниже подтверждают, что {$text4}
