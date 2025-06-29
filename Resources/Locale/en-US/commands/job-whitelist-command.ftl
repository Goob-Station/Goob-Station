# SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Tunguso4ka <71643624+Tunguso4ka@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-jobwhitelist-job-does-not-exist = Job {$job} does not exist.
cmd-jobwhitelist-player-not-found = Player {$player} not found.
cmd-jobwhitelist-hint-player = [player]
cmd-jobwhitelist-hint-job = [job]

cmd-jobwhitelistadd-desc = Lets a player play a whitelisted job.
cmd-jobwhitelistadd-help = Usage: jobwhitelistadd <username> <job>
cmd-jobwhitelistadd-already-whitelisted = {$player} is already whitelisted to play as {$jobId} .({$jobName}).
cmd-jobwhitelistadd-added = Added {$player} to the {$jobId} ({$jobName}) whitelist.

cmd-jobwhitelistget-desc = Gets all the jobs that a player has been whitelisted for.
cmd-jobwhitelistget-help = Usage: jobwhitelistget <username>
cmd-jobwhitelistget-whitelisted-none = Player {$player} is not whitelisted for any jobs.
cmd-jobwhitelistget-whitelisted-for = "Player {$player} is whitelisted for:
{$jobs}"

cmd-jobwhitelistremove-desc = Removes a player's ability to play a whitelisted job.
cmd-jobwhitelistremove-help = Usage: jobwhitelistremove <username> <job>
cmd-jobwhitelistremove-was-not-whitelisted = {$player} was not whitelisted to play as {$jobId} ({$jobName}).
cmd-jobwhitelistremove-removed = Removed {$player} from the whitelist for {$jobId} ({$jobName}).
