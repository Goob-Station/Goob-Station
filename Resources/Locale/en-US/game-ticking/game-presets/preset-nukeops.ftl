# SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Interrobang01 <113810873+Interrobang01@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 2013HORSEMEATSCANDAL <146540817+2013HORSEMEATSCANDAL@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2024 Avalon <148660190+BYONDFuckery@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

nukeops-title = Nuclear Operatives
nukeops-description = Nuclear operatives have targeted the station. Try to keep them from arming and detonating the nuke by protecting the nuke disk!

nukeops-welcome =
    You are a nuclear operative. Your goal is to blow up {$station}, and ensure that it is nothing but a pile of rubble. Your bosses, the Syndicate, have provided you with the tools you'll need for the task.
    Operation {$name} is a go ! Death to Nanotrasen!
nukeops-briefing = Your objectives are simple. Deliver the payload and get out before the payload detonates. Begin mission.

nukeops-opsmajor = [color=crimson]Syndicate major victory![/color]
nukeops-opsminor = [color=crimson]Syndicate minor victory![/color]
nukeops-neutral = [color=yellow]Neutral outcome![/color]
nukeops-crewminor = [color=green]Crew minor victory![/color]
nukeops-crewmajor = [color=green]Crew major victory![/color]

nukeops-cond-nukeexplodedoncorrectstation = The nuclear operatives managed to blow up the station.
nukeops-cond-nukeexplodedonnukieoutpost = The nuclear operative outpost was destroyed by a nuclear blast.
nukeops-cond-nukeexplodedonincorrectlocation = The nuclear bomb was detonated off-station.
nukeops-cond-nukeactiveinstation = The nuclear bomb was left armed on-station.
nukeops-cond-nukeactiveatcentcom = The nuclear bomb was delivered to Central Command!
nukeops-cond-nukediskoncentcom = The crew escaped with the nuclear authentication disk.
nukeops-cond-nukedisknotoncentcom = The crew left the nuclear authentication disk behind.
nukeops-cond-nukiesabandoned = The nuclear operatives were abandoned.
nukeops-cond-allnukiesdead = All nuclear operatives have died.
nukeops-cond-somenukiesalive = Some nuclear operatives died.
nukeops-cond-allnukiesalive = No nuclear operatives died.

nukeops-list-start = The operatives were:
nukeops-list-name = - [color=White]{$name}[/color]
nukeops-list-name-user = - [color=White]{$name}[/color] ([color=gray]{$user}[/color])
nukeops-not-enough-ready-players = Not enough players readied up for the game! There were {$readyPlayersCount} players readied up out of {$minimumPlayers} needed. Can't start Nukeops.
nukeops-no-one-ready = No players readied up! Can't start Nukeops.

nukeops-role-commander = Commander
nukeops-role-agent = Agent
nukeops-role-operator = Operator
