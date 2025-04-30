# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Chat window telephone wrap (prefix and postfix)
chat-telephone-message-wrap = [color={$color}][bold]{$name}[/bold] {$verb}, [font={$fontType} size={$fontSize}]"{$message}"[/font][/color]
chat-telephone-message-wrap-bold = [color={$color}][bold]{$name}[/bold] {$verb}, [font={$fontType} size={$fontSize}][bold]"{$message}"[/bold][/font][/color]

# Caller ID
chat-telephone-unknown-caller = [color={$color}][font={$fontType} size={$fontSize}][bolditalic]Unknown caller[/bolditalic][/font][/color]
chat-telephone-caller-id-with-job = [color={$color}][font={$fontType} size={$fontSize}][bold]{CAPITALIZE($callerName)} ({CAPITALIZE($callerJob)})[/bold][/font][/color]
chat-telephone-caller-id-without-job = [color={$color}][font={$fontType} size={$fontSize}][bold]{CAPITALIZE($callerName)}[/bold][/font][/color]
chat-telephone-unknown-device = [color={$color}][font={$fontType} size={$fontSize}][bolditalic]Unknown source[/bolditalic][/font][/color]
chat-telephone-device-id = [color={$color}][font={$fontType} size={$fontSize}][bold]{CAPITALIZE($deviceName)}[/bold][/font][/color]

# Chat text
chat-telephone-name-relay = {$originalName} ({$speaker})