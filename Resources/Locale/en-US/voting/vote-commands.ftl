# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Voting system related console commands

## 'createvote' command

cmd-createvote-desc = Creates a vote
cmd-createvote-help = Usage: createvote <'restart'|'preset'|'map'>
cmd-createvote-cannot-call-vote-now = You can't call a vote right now!
cmd-createvote-invalid-vote-type = Invalid vote type
cmd-createvote-arg-vote-type = <vote type>

## 'customvote' command

cmd-customvote-desc = Creates a custom vote
cmd-customvote-help = Usage: customvote <title> <option1> <option2> [option3...]
cmd-customvote-on-finished-tie = Tie between {$ties}!
cmd-customvote-on-finished-win = {$winner} wins!
cmd-customvote-arg-title = <title>
cmd-customvote-arg-option-n = <option{ $n }>

## 'vote' command

cmd-vote-desc = Votes on an active vote
cmd-vote-help = vote <voteId> <option>
cmd-vote-cannot-call-vote-now = You can't call a vote right now!
cmd-vote-on-execute-error-must-be-player = Must be a player
cmd-vote-on-execute-error-invalid-vote-id = Invalid vote ID
cmd-vote-on-execute-error-invalid-vote-options = Invalid vote options
cmd-vote-on-execute-error-invalid-vote = Invalid vote
cmd-vote-on-execute-error-invalid-option = Invalid option

## 'listvotes' command

cmd-listvotes-desc = Lists currently active votes
cmd-listvotes-help = Usage: listvotes

## 'cancelvote' command

cmd-cancelvote-desc = Cancels an active vote
cmd-cancelvote-help = Usage: cancelvote <id>
                      You can get the ID from the listvotes command.
cmd-cancelvote-error-invalid-vote-id = Invalid vote ID
cmd-cancelvote-error-missing-vote-id = Missing ID
cmd-cancelvote-arg-id = <id>
