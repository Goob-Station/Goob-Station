# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Kara Dinyes <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Michael Phillips <1194692+MeltedPixel@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morbo <exstrominer@gmail.com>
# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
# SPDX-FileCopyrightText: 2023 Errant <35878406+dmnct@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nairod <110078045+Nairodian@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2023 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deathride58 <deathride58@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 chavonadelal <156101927+chavonadelal@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
# SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
# SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### UI

chat-manager-max-message-length = Ваше сообщение превышает лимит в { $maxMessageLength } символов
chat-manager-ooc-chat-enabled-message = OOC чат был включён.
chat-manager-ooc-chat-disabled-message = OOC чат был отключён.
chat-manager-looc-chat-enabled-message = LOOC чат был включён.
chat-manager-looc-chat-disabled-message = LOOC чат был отключён.
chat-manager-dead-looc-chat-enabled-message = Мёртвые игроки теперь могут говорить в LOOC.
chat-manager-dead-looc-chat-disabled-message = Мёртвые игроки больше не могут говорить в LOOC.
chat-manager-crit-looc-chat-enabled-message = Игроки в критическом состоянии теперь могут говорить в LOOC.
chat-manager-crit-looc-chat-disabled-message = Игроки в критическом состоянии больше не могут говорить в LOOC.
chat-manager-admin-ooc-chat-enabled-message = Админ OOC чат был включён.
chat-manager-admin-ooc-chat-disabled-message = Админ OOC чат был выключен.

chat-manager-max-message-length-exceeded-message = Ваше сообщение превышает лимит в { $limit } символов
chat-manager-no-headset-on-message = У вас нет гарнитуры!
chat-manager-no-radio-key = Не задан ключ канала!
chat-manager-no-such-channel = Нет канала с ключём '{ $key }'!
chat-manager-whisper-headset-on-message = Вы не можете шептать в радио!

chat-manager-server-wrap-message = [bold]{$message}[/bold]
chat-manager-sender-announcement = Центральное командование
chat-manager-sender-announcement-wrap-message = [font size=14][bold]Объявление { $sender }:[/font][font size=12]
    { $message }[/bold][/font]
# Einstein Engines - Language begin (changing colors for text based on language color in handler)
# For the message in double quotes, the font/color/bold/italic elements are repeated twice, outside the double quotes and inside.
# The outside elements are for formatting the double quotes, and the inside elements are for formatting the text in speech bubbles ([BubbleContent]).
chat-manager-entity-say-wrap-message = [BubbleHeader][bold][Name]{ $entityName }[/Name][/bold][/BubbleHeader] { $verb }, [font={ $fontType } size={ $fontSize }]"[BubbleContent][font="{ $fontType }" size={ $fontSize }][color={ $color }]{ $message }[/color][/font][/BubbleContent]"[/font]
chat-manager-entity-say-bold-wrap-message = [BubbleHeader][bold][Name]{ $entityName }[/Name][/bold][/BubbleHeader] { $verb }, [font={ $fontType } size={ $fontSize }]"[BubbleContent][font="{ $fontType }" size={ $fontSize }][bold][color={ $color }]{ $message }[/color][/bold][/font][/BubbleContent]"[/font]

chat-manager-entity-whisper-wrap-message = [font size=11][italic][BubbleHeader][Name]{ $entityName }[/Name][/BubbleHeader] шепчет, "[BubbleContent][color={ $color }][font="{ $fontType }"]{ $message }[/font][/color][/BubbleContent][font size=11]"[/italic][/font]
chat-manager-entity-whisper-unknown-wrap-message = [font size=11][italic][BubbleHeader]Кто-то[/BubbleHeader] шепчет, "[BubbleContent][color={ $color }][font="{ $fontType }"]{ $message }[/color][/font][/BubbleContent][font size=11]"[/italic][/font]
# Einstein Engines - Language end

# chat-manager-language-prefix = ({ $language }){" "} - Removed so it doesn't show up, not wanted, but part of the language system.

# THE() is not used here because the entity and its name can technically be disconnected if a nameOverride is passed...
chat-manager-entity-me-wrap-message = [italic]{ CAPITALIZE($entityName) } { $message }[/italic]

chat-manager-entity-looc-wrap-message = LOOC: [bold]{ $entityName }:[/bold] { $message }
chat-manager-send-ooc-wrap-message = OOC: [bold]{ $playerName }:[/bold] { $message }
chat-manager-send-ooc-patron-wrap-message = OOC: [bold][color={ $patronColor }]{ $playerName }[/color]:[/bold] { $message }
chat-manager-send-ooc-patron-wrap-message-no-icon = OOC: [bold][color={ $patronColor }]{ $playerName }[/color]:[/bold] { $message }

chat-manager-send-dead-chat-wrap-message = { $deadChannelName }: [bold][BubbleHeader]{ $playerName }[/BubbleHeader]:[/bold] [BubbleContent]{ $message }[/BubbleContent]
chat-manager-send-admin-dead-chat-wrap-message = { $adminChannelName }: [bold]([BubbleHeader]{ $userName }[/BubbleHeader]):[/bold] [BubbleContent]{ $message }[/BubbleContent]
chat-manager-send-admin-chat-wrap-message = { $adminChannelName }: [bold]{ $playerName }:[/bold] { $message }
chat-manager-send-admin-announcement-wrap-message = [bold]{ $adminChannelName }: { $message }[/bold]

chat-manager-send-hook-ooc-wrap-message = OOC: [bold](D){ $senderName }:[/bold] { $message }

chat-manager-dead-channel-name = МЁРТВЫЕ
chat-manager-admin-channel-name = АДМИН

chat-manager-rate-limited = Вы отправляете сообщения слишком быстро!
chat-manager-rate-limit-admin-announcement = Предупреждение о превышении ограничения скорости: { $player }

## Speech verbs for chat

chat-speech-verb-suffix-exclamation = !
chat-speech-verb-suffix-exclamation-strong = !!
chat-speech-verb-suffix-question = ?
chat-speech-verb-suffix-stutter = -
chat-speech-verb-suffix-mumble = ..

chat-speech-verb-name-none = Нет
chat-speech-verb-name-default = По умолчанию
chat-speech-verb-default = говорит
chat-speech-verb-name-exclamation = Восклицание
chat-speech-verb-exclamation = восклицает
chat-speech-verb-name-exclamation-strong = Крик
chat-speech-verb-exclamation-strong = кричит
chat-speech-verb-name-question = Вопрос
chat-speech-verb-question = спрашивает
chat-speech-verb-name-stutter = Заикание
chat-speech-verb-stutter = запинается
chat-speech-verb-name-mumble = Бубнёж
chat-speech-verb-mumble = бубнит

chat-speech-verb-name-arachnid = Арахнид
chat-speech-verb-insect-1 = стрекочет
chat-speech-verb-insect-2 = жужжит
chat-speech-verb-insect-3 = щёлкает

chat-speech-verb-name-moth = Ниан
chat-speech-verb-winged-1 = свистит
chat-speech-verb-winged-2 = хлопает
chat-speech-verb-winged-3 = клокочет

chat-speech-verb-name-slime = Слаймолюд
chat-speech-verb-slime-1 = шлёпает
chat-speech-verb-slime-2 = бурлит
chat-speech-verb-slime-3 = булькает

chat-speech-verb-name-plant = Диона
chat-speech-verb-plant-1 = шелестит
chat-speech-verb-plant-2 = шуршит
chat-speech-verb-plant-3 = скрипит

chat-speech-verb-name-robotic = Робот
chat-speech-verb-robotic-1 = докладывает
chat-speech-verb-robotic-2 = пищит
chat-speech-verb-robotic-3 = информирует

chat-speech-verb-name-reptilian = Унатх
chat-speech-verb-reptilian-1 = шипит
chat-speech-verb-reptilian-2 = фыркает
chat-speech-verb-reptilian-3 = пыхтит

chat-speech-verb-name-skeleton = Скелет
chat-speech-verb-skeleton-1 = гремит
chat-speech-verb-skeleton-2 = клацает
chat-speech-verb-skeleton-3 = скрежещет
chat-speech-verb-skeleton-4 = клацкает
chat-speech-verb-skeleton-5 = клацклацкает

chat-speech-verb-name-vox = Вокс
chat-speech-verb-vox-1 = скрипит
chat-speech-verb-vox-2 = визжит
chat-speech-verb-vox-3 = каркает

chat-speech-verb-name-canine = Собака
chat-speech-verb-canine-1 = гавкает
chat-speech-verb-canine-2 = лает
chat-speech-verb-canine-3 = воет

chat-speech-verb-name-goat = Коза
chat-speech-verb-goat-1 = блеет
chat-speech-verb-goat-2 = кряхтит
chat-speech-verb-goat-3 = кричит

chat-speech-verb-name-small-mob = Мышь
chat-speech-verb-small-mob-1 = скрипит
chat-speech-verb-small-mob-2 = пищит

chat-speech-verb-name-large-mob = Карп
chat-speech-verb-large-mob-1 = ревёт
chat-speech-verb-large-mob-2 = рычит

chat-speech-verb-name-monkey = Обезьяна
chat-speech-verb-monkey-1 = обезьяничает
chat-speech-verb-monkey-2 = визжит

chat-speech-verb-name-cluwne = Клувень

chat-speech-verb-name-parrot = Попугай
chat-speech-verb-parrot-1 = кричит
chat-speech-verb-parrot-2 = чирикает
chat-speech-verb-parrot-3 = щебечет

chat-speech-verb-cluwne-1 = хихикает
chat-speech-verb-cluwne-2 = хехекает
chat-speech-verb-cluwne-3 = смеётся

chat-speech-verb-name-ghost = Призрак
chat-speech-verb-ghost-1 = жалуется
chat-speech-verb-ghost-2 = вздыхает
chat-speech-verb-ghost-3 = гудит
chat-speech-verb-ghost-4 = бормочет

chat-speech-verb-name-electricity = Электричество
chat-speech-verb-electricity-1 = трещит
chat-speech-verb-electricity-2 = гудит
chat-speech-verb-electricity-3 = скрипит

chat-speech-verb-name-wawa = Вава
chat-speech-verb-wawa-1 = напевает
chat-speech-verb-wawa-2 = утверждает
chat-speech-verb-wawa-3 = заявляет
chat-speech-verb-wawa-4 = размышляет
