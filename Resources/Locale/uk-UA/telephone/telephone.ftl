# Chat window telephone wrap (prefix and postfix)
chat-telephone-message-wrap = [color={$color}][bold]{$name}[/bold] {$verb}, [font={$fontType} size={$fontSize}]"{$message}"[/font][/color]
chat-telephone-message-wrap-bold = [color={$color}][bold]{$name}[/bold] {$verb}, [font={$fontType} size={$fontSize}][bold]"{$message}"[/bold][/font][/color]
chat-telephone-unknown-caller = [color={$color}][font={$fontType} size={$fontSize}][bolditalic]Невідомий абонент[/bolditalic][/font][/color]
chat-telephone-caller-id-with-job = [color={$color}][font={$fontType} size={$fontSize}][bold]{CAPITALIZE($callerName)} ({CAPITALIZE($callerJob)})[/bold][/font][/color]
chat-telephone-caller-id-without-job = [color={$color}][font={$fontType} size={$fontSize}][bold]{CAPITALIZE($callerName)}[/bold][/font][/color]
chat-telephone-unknown-device = [color={$color}][font={$fontType} size={$fontSize}][bolditalic]Невідомий пристрій[/bolditalic][/font][/color]
chat-telephone-device-id = [color={$color}][font={$fontType} size={$fontSize}][bold]{CAPITALIZE($deviceName)}[/bold][/font][/color]
chat-telephone-name-relay = {$originalName} ({$speaker})