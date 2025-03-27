# templates
# service
service-starting-text = [color=#009100]█▄ █ ▀█▀    [head=3]NanoTrasen Document[/head]
    █ ▀█     █        To: Service department
                           From: CentComm[/color]
    ──────────────────────────────────────────

# security
security-starting-text = [head=3]NanoTrasen Document[/head]                               [color=#990909]█▄ █ ▀█▀
    To: Security department                                       █ ▀█     █
    From: CentComm[/color]
    ──────────────────────────────────────────

# cargo
cargo-starting-text = [head=3]  NanoTrasen[/head]        [color=#d48311]█▄ █ ▀█▀ [/color][bold]      To: Cargo department[/bold][head=3]
       Document[/head]           [color=#d48311]█ ▀█     █       [/color] [bold]   From: CentComm[/bold]
    ──────────────────────────────────────────
# medical
medical-starting-text = [color=#118fd4]░             █▄ █ ▀█▀    [head=3]NanoTrasen Document[/head]                 ░
    █             █ ▀█     █        To: Medical department                         █
    ░                                    From: CentComm[/color]                                     ░
    ──────────────────────────────────────────

# engineering
engineering-starting-text = [color=#a15000]█▄ █ ▀█▀    [head=3]NanoTrasen Document[/head]
    █ ▀█     █        To: Engineering department
                           From: CentComm[/color]
    ──────────────────────────────────────────

service-document-text =
    {$start}
    Corporate wants you to know that you are not {$text1} {$text2}
    Corporate would be pleased if you  {$text3}
    Stamps below confirm that {$text4}

security-document-text =
    {$start}
    Corporate wants you to check some stuff before stamping this document, make sure that {$text1} {$text2}
    {$text3}
    {$text4}

cargo-document-text =
    {$start}
    {$text1}
    {$text2}
    By stamping here, you {$text3}

medical-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    By stamping here, you {$text4}

engineering-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    By stamping here, you {$text4}
