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

chat-manager-max-message-length = Votre message dépasse la limite de {$maxMessageLength} caractères
chat-manager-ooc-chat-enabled-message = Le chat OOC a été activé.
chat-manager-ooc-chat-disabled-message = Le chat OOC a été désactivé.
chat-manager-looc-chat-enabled-message = Le chat LOOC a été activé.
chat-manager-looc-chat-disabled-message = Le chat LOOC a été désactivé.
chat-manager-dead-looc-chat-enabled-message = Les joueurs morts peuvent maintenant utiliser le LOOC.
chat-manager-dead-looc-chat-disabled-message = Les joueurs morts ne peuvent plus utiliser le LOOC.
chat-manager-crit-looc-chat-enabled-message = Les joueurs en état critique peuvent maintenant utiliser le LOOC.
chat-manager-crit-looc-chat-disabled-message = Les joueurs en état critique ne peuvent plus utiliser le LOOC.
chat-manager-admin-ooc-chat-enabled-message = Le chat OOC admin a été activé.
chat-manager-admin-ooc-chat-disabled-message = Le chat OOC admin a été désactivé.

chat-manager-max-message-length-exceeded-message = Votre message a dépassé la limite de {$limit} caractères
chat-manager-no-headset-on-message = Vous n'avez pas de casque !
chat-manager-no-radio-key = Aucune touche radio spécifiée !
chat-manager-no-such-channel = Il n'existe pas de canal avec la touche '{$key}' !
chat-manager-whisper-headset-on-message = Vous ne pouvez pas chuchoter à la radio !

chat-manager-server-wrap-message = [bold]{$message}[/bold]
chat-manager-sender-announcement = Commandement central
chat-manager-sender-announcement-wrap-message = [font size=14][bold]{$sender} Announcement:[/font][font size=12]
                                                {$message}[/bold][/font]
# Einstein Engines - Language begin (changing colors for text based on language color in handler)
# For the message in double quotes, the font/color/bold/italic elements are repeated twice, outside the double quotes and inside.
# The outside elements are for formatting the double quotes, and the inside elements are for formatting the text in speech bubbles ([BubbleContent]).
chat-manager-entity-say-wrap-message = [BubbleHeader][bold][Name]{$entityName}[/Name][/bold][/BubbleHeader] {$verb}, [font={$fontType} size={$fontSize}]"[BubbleContent][font="{$fontType}" size={$fontSize}][color={$color}]{$message}[/color][/font][/BubbleContent]"[/font]
chat-manager-entity-say-bold-wrap-message = [BubbleHeader][bold][Name]{$entityName}[/Name][/bold][/BubbleHeader] {$verb}, [font={$fontType} size={$fontSize}]"[BubbleContent][font="{$fontType}" size={$fontSize}][bold][color={$color}]{$message}[/color][/bold][/font][/BubbleContent]"[/font]

chat-manager-entity-whisper-wrap-message = [font size=11][italic][BubbleHeader][Name]{$entityName}[/Name][/BubbleHeader] whispers, "[BubbleContent][color={$color}][font="{$fontType}"]{$message}[/font][/color][/BubbleContent][font size=11]"[/italic][/font]
chat-manager-entity-whisper-unknown-wrap-message = [font size=11][italic][BubbleHeader]Someone[/BubbleHeader] whispers, "[BubbleContent][color={$color}][font="{$fontType}"]{$message}[/color][/font][/BubbleContent][font size=11]"[/italic][/font]
# Einstein Engines - Language end

# chat-manager-language-prefix = ({ $language }){" "} - Removed so it doesn't show up, not wanted, but part of the language system.

# THE() is not used here because the entity and its name can technically be disconnected if a nameOverride is passed...
chat-manager-entity-me-wrap-message = [italic]{ PROPER($entity) ->
    *[false] The {$entityName} {$message}[/italic]
     [true] {CAPITALIZE($entityName)} {$message}[/italic]
    }

chat-manager-entity-looc-wrap-message = LOOC: [bold]{$entityName}:[/bold] {$message}
chat-manager-send-ooc-wrap-message = OOC: [bold]{$playerName}:[/bold] {$message}
chat-manager-send-ooc-patron-wrap-message = OOC: [icon src="{$tierIcon}"/] [bold][color={$patronColor}]{$playerName}[/color]:[/bold] {$message}
chat-manager-send-ooc-patron-wrap-message-no-icon = OOC: [bold][color={$patronColor}]{$playerName}[/color]:[/bold] {$message}

chat-manager-send-dead-chat-wrap-message = {$deadChannelName}: [bold][BubbleHeader]{$playerName}[/BubbleHeader][/bold] {$verb}: "[BubbleContent]{$message}[/BubbleContent]"
chat-manager-send-admin-dead-chat-wrap-message = {$adminChannelName}: [bold]([BubbleHeader]{$userName}[/BubbleHeader])[/bold] {$verb}: "[BubbleContent]{$message}[/BubbleContent]"
chat-manager-send-admin-chat-wrap-message = {$adminChannelName}: [bold]{$playerName}:[/bold] {$message}
chat-manager-send-admin-announcement-wrap-message = [bold]{$adminChannelName}: {$message}[/bold]

chat-manager-send-hook-ooc-wrap-message = OOC: [bold](D){$senderName}:[/bold] {$message}

chat-manager-dead-channel-name = DEAD
chat-manager-admin-channel-name = ADMIN

chat-manager-rate-limited = Vous envoyez des messages trop rapidement !
chat-manager-rate-limit-admin-announcement = Rate limit warning: { $player }

## Speech verbs for chat

chat-speech-verb-suffix-exclamation = !
chat-speech-verb-suffix-exclamation-strong = !!
chat-speech-verb-suffix-question = ?
chat-speech-verb-suffix-stutter = -
chat-speech-verb-suffix-mumble = ..

chat-speech-verb-name-none = Aucun
chat-speech-verb-name-default = Défaut
chat-speech-verb-default = dit
chat-speech-verb-name-exclamation = Exclamation
chat-speech-verb-exclamation = s'exclame
chat-speech-verb-name-exclamation-strong = Cri
chat-speech-verb-exclamation-strong = crie
chat-speech-verb-name-question = Question
chat-speech-verb-question = demande
chat-speech-verb-name-stutter = Bégaiement
chat-speech-verb-stutter = bégaie
chat-speech-verb-name-mumble = Marmonnement
chat-speech-verb-mumble = marmonne

chat-speech-verb-name-arachnid = Arachnide
chat-speech-verb-insect-1 = claque
chat-speech-verb-insect-2 = pépie
chat-speech-verb-insect-3 = claque

chat-speech-verb-name-moth = Mite
chat-speech-verb-winged-1 = volète
chat-speech-verb-winged-2 = bat des ailes
chat-speech-verb-winged-3 = bourdonne

chat-speech-verb-name-slime = Slime
chat-speech-verb-slime-1 = clapote
chat-speech-verb-slime-2 = gargouille
chat-speech-verb-slime-3 = suinte

chat-speech-verb-name-plant = Diona
chat-speech-verb-plant-1 = bruisse
chat-speech-verb-plant-2 = se balance
chat-speech-verb-plant-3 = craque

chat-speech-verb-name-robotic = Robotique
chat-speech-verb-robotic-1 = déclare
chat-speech-verb-robotic-2 = bipe
chat-speech-verb-robotic-3 = bippe

chat-speech-verb-name-reptilian = Reptilien
chat-speech-verb-reptilian-1 = siffle
chat-speech-verb-reptilian-2 = renifle
chat-speech-verb-reptilian-3 = souffle

chat-speech-verb-name-skeleton = Squelette / Plasmoïde
chat-speech-verb-skeleton-1 = s'entrechoque
chat-speech-verb-skeleton-2 = crécelle
chat-speech-verb-skeleton-3 = cliquète
chat-speech-verb-skeleton-4 = claque
chat-speech-verb-skeleton-5 = craque

chat-speech-verb-name-vox = Vox
chat-speech-verb-vox-1 = crie
chat-speech-verb-vox-2 = croasse
chat-speech-verb-vox-3 = coasse

chat-speech-verb-name-canine = Canin
chat-speech-verb-canine-1 = aboie
chat-speech-verb-canine-2 = ouafe
chat-speech-verb-canine-3 = hurle

chat-speech-verb-name-goat = Chèvre
chat-speech-verb-goat-1 = bêle
chat-speech-verb-goat-2 = grogne
chat-speech-verb-goat-3 = pleure

chat-speech-verb-name-small-mob = Souris
chat-speech-verb-small-mob-1 = couine
chat-speech-verb-small-mob-2 = piaille

chat-speech-verb-name-large-mob = Carpe
chat-speech-verb-large-mob-1 = rugit
chat-speech-verb-large-mob-2 = gronde

chat-speech-verb-name-monkey = Singe
chat-speech-verb-monkey-1 = singe
chat-speech-verb-monkey-2 = crie

chat-speech-verb-name-cluwne = Cluwne
chat-speech-verb-cluwne-1 = glousse
chat-speech-verb-cluwne-2 = se marre
chat-speech-verb-cluwne-3 = rit

chat-speech-verb-name-parrot = Perroquet
chat-speech-verb-parrot-1 = couine
chat-speech-verb-parrot-2 = gazouille
chat-speech-verb-parrot-3 = pépie

chat-speech-verb-name-ghost = Fantôme
chat-speech-verb-ghost-1 = se plaint
chat-speech-verb-ghost-2 = respire
chat-speech-verb-ghost-3 = fredonne
chat-speech-verb-ghost-4 = marmonne

chat-speech-verb-name-electricity = Électricité
chat-speech-verb-electricity-1 = crépite
chat-speech-verb-electricity-2 = bourdonne
chat-speech-verb-electricity-3 = crie

chat-speech-verb-name-wawa = Wawa
chat-speech-verb-wawa-1 = entonne
chat-speech-verb-wawa-2 = déclare
chat-speech-verb-wawa-3 = déclare
chat-speech-verb-wawa-4 = réfléchit
