### UI

chat-manager-max-message-length = Довжина вашого повідомлення перевищує ліміт у {$maxMessageLength} символів
chat-manager-ooc-chat-enabled-message = OOC увімкнено.
chat-manager-ooc-chat-disabled-message = OOC вимкнено.
chat-manager-looc-chat-enabled-message = LOOC увімкнено.
chat-manager-looc-chat-disabled-message = LOOC вимкнено.
chat-manager-dead-looc-chat-enabled-message = Мерці тепер можуть використовувати LOOC.
chat-manager-dead-looc-chat-disabled-message = Мерці тепер не можуть використовувати LOOC.
chat-manager-crit-looc-chat-enabled-message = Гравці у криті можуть користуватись LOOC.
chat-manager-crit-looc-chat-disabled-message = Гравці у криті більше не можуть користуватись LOOC.
chat-manager-admin-ooc-chat-enabled-message = Адмінське OOC увімнено.
chat-manager-admin-ooc-chat-disabled-message = Адмінське OOC вимкнено.

chat-manager-max-message-length-exceeded-message = Ваше повідомлення перевищило ліміт у {$limit} символів
chat-manager-no-headset-on-message = У вас немає гарнітури!
chat-manager-no-radio-key = Ви вибрано радіо канал!
chat-manager-no-such-channel = Немає такого каналу '{$key}'!
chat-manager-whisper-headset-on-message = По радіо не можна шепотіти!

chat-manager-server-wrap-message = [bold]{$message}[/bold]
chat-manager-sender-announcement-wrap-message = [font size=14][bold]Оголошення {$sender}:[/font][font size=12]
                                                {$message}[/bold][/font]
chat-manager-entity-say-wrap-message = [BubbleHeader][Name][font size=11][color={$color}][bold]{$language}[/bold][/color][/font][bold]{$entityName}[/bold][/Name][/BubbleHeader] {$verb}, [font="{$fontType}" size={$fontSize} ][color={$color}]"[BubbleContent][font="{$fontType}" size={$fontSize}][color={$color}]{$message}[/color][/font][/BubbleContent]"[/color][/font]
chat-manager-entity-say-bold-wrap-message = [BubbleHeader][Name][font size=11][color={$color}][bold]{$language}[/bold][/color][/font][bold]{$entityName}[/bold][/Name][/BubbleHeader] {$verb}, [font="{$fontType}" size={$fontSize} ][color={$color}][bold]"[BubbleContent][font="{$fontType}" size={$fontSize}][color={$color}][bold]{$message}[/bold][/color][/font][/BubbleContent]"[/bold][/color][/font]

chat-manager-entity-whisper-wrap-message = [BubbleHeader][Name][font size=10][color={$color}][bold]{$language}[/bold][/color][/font][font size=11][italic]{$entityName}[/Name][/BubbleHeader] шепоче, [font="{$fontType}"][color={$color}][italic]"[BubbleContent][font="{$fontType}"][color={$color}][italic]{$message}[/italic][/color][/font][/BubbleContent]"[/italic][/color][/font][/italic][/font]
chat-manager-entity-whisper-unknown-wrap-message = [BubbleHeader][font size=10][color={$color}][bold]{$language}[/bold][/color][/font][font size=11][italic]Хтось[/BubbleHeader] шепоче, [font="{$fontType}"][color={$color}][italic]"[BubbleContent][font="{$fontType}"][color={$color}][italic]{$message}[/italic][/color][/font][/BubbleContent]"[/italic][/color][/font][/italic][/font]

# THE() тут не використовується, тому що сутність та її ім'я технічно можна роз'єднати, якщо передати nameOverride...
chat-manager-entity-me-wrap-message = [italic]{ PROPER($entity) ->
    *[false] {$entityName} {$message}[/italic]
     [true] {$entityName} {$message}[/italic]
    }

chat-manager-entity-looc-wrap-message = LOOC: {$entityName}: {$message}
chat-manager-send-ooc-wrap-message = OOC: {$playerName}: {$message}
chat-manager-send-ooc-patron-wrap-message = OOC: [color={$patronColor}]{$playerName}[/color]: {$message}

chat-manager-send-dead-chat-wrap-message = {$deadChannelName}: [BubbleHeader]{$playerName}[/BubbleHeader]: [BubbleContent]{$message}[/BubbleContent]
chat-manager-send-admin-dead-chat-wrap-message = {$adminChannelName}: ([BubbleHeader]{$userName}[/BubbleHeader]): [BubbleContent]{$message}[/BubbleContent]
chat-manager-send-admin-chat-wrap-message = {$adminChannelName}: {$playerName}: {$message}
chat-manager-send-admin-announcement-wrap-message = [bold]{$adminChannelName}: {$message}[/bold]

chat-manager-send-hook-ooc-wrap-message = OOC: (D){$senderName}: {$message}

chat-manager-dead-channel-name = МЕРТВІ
chat-manager-admin-channel-name = АДМІН

chat-manager-rate-limited = Ви надсилаєте повідомлення надто швидко!
chat-manager-rate-limit-admin-announcement = Гравець { $player } перевищив ліміт повідомлень на секунду. Слідкуйте за ним, якщо це трапляється часто.

## Speech verbs for chat

chat-speech-verb-suffix-exclamation = !
chat-speech-verb-suffix-exclamation-strong = !!
chat-speech-verb-suffix-question = ?
chat-speech-verb-suffix-stutter = -
chat-speech-verb-suffix-mumble = ..

chat-speech-verb-default = каже
chat-speech-verb-exclamation = вигукує
chat-speech-verb-exclamation-strong = кричить
chat-speech-verb-question = питає
chat-speech-verb-stutter = заїкається
chat-speech-verb-mumble = бурмочить

chat-speech-verb-insect-1 = лепече
chat-speech-verb-insect-2 = цвірінькає
chat-speech-verb-insect-3 = клацає

chat-speech-verb-winged-1 = пурхає
chat-speech-verb-winged-2 = клапає
chat-speech-verb-winged-3 = дзижчить

chat-speech-verb-slime-1 = хлюпає
chat-speech-verb-slime-2 = бормоче
chat-speech-verb-slime-3 = булькає

chat-speech-verb-plant-1 = шелестить
chat-speech-verb-plant-2 = коливається
chat-speech-verb-plant-3 = скрипить

chat-speech-verb-robotic-1 = константує
chat-speech-verb-robotic-2 = бібупає

chat-speech-verb-reptilian-1 = шипить
chat-speech-verb-reptilian-2 = фиркає
chat-speech-verb-reptilian-3 = видуває

chat-speech-verb-skeleton-1 = гримить
chat-speech-verb-skeleton-2 = клацає
chat-speech-verb-skeleton-3 = скреготить

chat-speech-verb-vox-1 = верещить
chat-speech-verb-vox-2 = кричить
chat-speech-verb-vox-3 = каркає

chat-speech-verb-canine-1 = гавкає
chat-speech-verb-canine-2 = вуфає
chat-speech-verb-canine-3 = виє

chat-speech-verb-small-mob-1 = пищить
chat-speech-verb-small-mob-2 = сопить

chat-speech-verb-large-mob-1 = реве
chat-speech-verb-large-mob-2 = гарчить

chat-speech-verb-monkey-1 = хрипить
chat-speech-verb-monkey-2 = верещить

chat-speech-verb-parrot-1 = крякає
chat-speech-verb-parrot-2 = цвірінькає
chat-speech-verb-parrot-3 = щебече

chat-speech-verb-cluwne-1 = гигикає
chat-speech-verb-cluwne-2 = регоче
chat-speech-verb-cluwne-3 = сміється

chat-speech-verb-ghost-1 = нарікає
chat-speech-verb-ghost-2 = видихує
chat-speech-verb-ghost-3 = гуде
chat-speech-verb-ghost-4 = бурмоче

chat-speech-verb-electricity-1 = потріскує
chat-speech-verb-electricity-2 = дзижчить
chat-speech-verb-electricity-3 = верещить

#PIRATE START
chat-manager-empathy-channel-name = ЕМПАТІЯ

chat-speech-verb-name-goblin = Гоблін

chat-speech-verb-name-none = Ні
chat-speech-verb-name-default = За замовчуванням
chat-speech-verb-name-exclamation = Вигукнув
chat-speech-verb-name-exclamation-strong = Крик!
chat-speech-verb-name-question = Просимо
chat-speech-verb-name-stutter = Заїкання
chat-speech-verb-name-mumble = Бурмотіння
chat-speech-verb-name-arachnid = Павукоподібний
chat-speech-verb-name-moth = Метелик
chat-speech-verb-name-slime = Слиз
chat-speech-verb-name-plant = Діона
chat-speech-verb-name-robotic = Робот
chat-speech-verb-robotic-3 = бум!
chat-speech-verb-name-reptilian = Рептилія
chat-speech-verb-name-skeleton = Скелет
chat-speech-verb-name-vox = Vox
chat-speech-verb-name-canine = Собачий
chat-speech-verb-name-small-mob = Миша
chat-speech-verb-name-large-mob = Короп
chat-speech-verb-name-monkey = Мавпа
chat-speech-verb-name-cluwne = Клюн
chat-speech-verb-name-parrot = Папуга
chat-speech-verb-name-ghost = Привид
chat-speech-verb-name-electricity = Електрика
chat-speech-verb-name-oni = Оні
chat-speech-verb-oni-1 = бурчить
chat-speech-verb-oni-2 = висвистує
chat-speech-verb-oni-3 = викрикує
chat-speech-verb-oni-4 = гуркоче
chat-speech-verb-marish = Марс.

#ПІРАТСЬКИЙ СТАРТ

chat-manager-send-empathy-chat-wrap-message = {$source}: {$message}
chat-manager-send-cult-chat-wrap-message = [bold]\[{ $channelName }\] [BubbleHeader]{ $player }[/BubbleHeader]:[/bold] [BubbleContent]{ $message }[/BubbleContent]
chat-manager-cult-channel-name = Культ Крові

chat-manager-language-prefix = ({ $language }){" "}

chat-speech-verb-skeleton-4 = клацання
chat-speech-verb-skeleton-5 = тріщини
chat-speech-verb-name-supermatter = Надматерія
chat-speech-verb-supermatter = стани

chat-speech-verb-Psychomantic-1 = резонує
chat-speech-verb-Psychomantic-2 = проєктує
chat-speech-verb-Psychomantic-3 = вражає
chat-speech-verb-Psychomantic-4 = випромінює
