# templates
# service
ntr-document-service-starting-text1 = [color=#009100]█▄ █ ▀█▀    [head=3]NanoTrasen Document[/head]
    █ ▀█     █        To: Service department
                           From: CentComm
                           Issued: {$date}
    ──────────────────────────────────────────[/color]

# security
ntr-document-security-starting-text1 = [head=3]NanoTrasen Document[/head]                               [color=#990909]█▄ █ ▀█▀
    To: Security department                                       █ ▀█     █
    From: CentComm
    Issued: {$date}
    ──────────────────────────────────────────[/color]

# cargo
ntr-document-cargo-starting-text1 = [head=3]  NanoTrasen[/head]        [color=#d48311]█▄ █ ▀█▀ [/color][bold]      To: Cargo department[/bold][head=3]
       Document[/head]           [color=#d48311]█ ▀█     █       [/color] [bold]   From: CentComm[/bold]
    ──────────────────────────────────────────
                                        Issued: {$date}

# medical
ntr-document-medical-starting-text1 = [color=#118fd4]░             █▄ █ ▀█▀    [head=3]NanoTrasen Document[/head]                 ░
    █             █ ▀█     █        To: Medical department                         █
    ░                                    From: CentComm                                     ░
                                         Issued: {$date}
    ──────────────────────────────────────────[/color]

# engineering
ntr-document-engineering-starting-text1 = [color=#a15000]█▄ █ ▀█▀    [head=3]NanoTrasen Document[/head]
    █ ▀█     █        To: Engineering department
                           From: CentComm
                           Issued: {$date}
    ──────────────────────────────────────────[/color]

# science
ntr-document-science-starting-text1 = [color=#94196f]░             █▄ █ ▀█▀    [head=3]NanoTrasen Document[/head]                 ░
    █             █ ▀█     █        To: Science department                         █
    ░                                    From: CentComm                                     ░
                                         Issued: {$date}
    ──────────────────────────────────────────[/color]
ntr-document-service-document-text =
    {$start}
    Corporate wants you to know that you are not {$text1} {$text2}
    Corporate would be pleased if you  {$text3}
    Stamps below confirm that {$text4}

ntr-document-security-document-text =
    {$start}
    Corporate wants you to check some stuff before stamping this document, make sure that {$text1} {$text2}
    {$text3}
    {$text4}

ntr-document-cargo-document-text =
    {$start}
    {$text1}
    {$text2}
    By stamping here, you {$text3}

ntr-document-medical-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    By stamping here, you {$text4}

ntr-document-engineering-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    By stamping here, you {$text4}

ntr-document-science-document-text =
    {$start}
    We have been closely monitoring the Research Department. {$text1} {$text2}
    due to everything above, we want you to ensure {$text3}
    stamps below confirm {$text4}
