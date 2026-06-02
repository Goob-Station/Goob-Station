# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# ban
cmd-ban-desc = Bans somebody
cmd-ban-help = Usage: ban <name or user ID> <reason> [duration in minutes, leave out or 0 for permanent ban]
cmd-ban-player = Unable to find a player with that name.
cmd-ban-invalid-minutes = {$minutes} is not a valid amount of minutes!
cmd-ban-invalid-severity = {$severity} is not a valid severity!
cmd-ban-invalid-arguments = Invalid amount of arguments
cmd-ban-hint = <name/user ID>
cmd-ban-hint-reason = <reason>
cmd-ban-hint-duration = [duration]
cmd-ban-hint-severity = [severity]

cmd-ban-hint-duration-1 = Permanent
cmd-ban-hint-duration-2 = 1 day
cmd-ban-hint-duration-3 = 3 days
cmd-ban-hint-duration-4 = 1 week
cmd-ban-hint-duration-5 = 2 week
cmd-ban-hint-duration-6 = 1 month

# ban panel
cmd-banpanel-desc = Opens the ban panel
cmd-banpanel-help = Usage: banpanel [name or user guid]
cmd-banpanel-server = This can not be used from the server console
cmd-banpanel-player-err = The specified player could not be found

# listbans
cmd-banlist-desc = Lists a user's active bans.
cmd-banlist-help = Usage: banlist <name or user ID>
cmd-banlist-empty = No active bans found for {$user}
cmd-banlist-hint = <name/user ID>

cmd-ban_exemption_update-desc = Set an exemption to a type of ban on a player.
cmd-ban_exemption_update-help = Usage: ban_exemption_update <player> <flag> [<flag> [...]]
    Specify multiple flags to give a player multiple ban exemption flags.
    To remove all exemptions, run this command and give "None" as only flag.

cmd-ban_exemption_update-nargs = Expected at least 2 arguments
cmd-ban_exemption_update-locate = Unable to locate player '{$player}'.
cmd-ban_exemption_update-invalid-flag = Invalid flag '{$flag}'.
cmd-ban_exemption_update-success = Updated ban exemption flags for '{$player}' ({$uid}).
cmd-ban_exemption_update-arg-player = <player>
cmd-ban_exemption_update-arg-flag = <flag>

cmd-ban_exemption_get-desc = Show ban exemptions for a certain player.
cmd-ban_exemption_get-help = Usage: ban_exemption_get <player>

cmd-ban_exemption_get-nargs = Expected exactly 1 argument
cmd-ban_exemption_get-none = User is not exempt from any bans.
cmd-ban_exemption_get-show = User is exempt from the following ban flags: {$flags}.
cmd-ban_exemption_get-arg-player = <player>

# Ban panel
ban-panel-title = Panneau de bannissement
ban-panel-player = Joueur
ban-panel-ip = IP
ban-panel-hwid = HWID
ban-panel-reason = Raison
ban-panel-last-conn = Utiliser l'IP et le HWID de la dernière connexion ?
ban-panel-submit = Bannir
ban-panel-confirm = Êtes-vous sûr ?
ban-panel-tabs-basic = Infos de base
ban-panel-tabs-reason = Raison
ban-panel-tabs-players = Liste des joueurs
ban-panel-tabs-role = Infos de ban de rôle
ban-panel-no-data = Vous devez fournir un utilisateur, une IP ou un HWID à bannir
ban-panel-invalid-ip = L'adresse IP n'a pas pu être analysée. Veuillez réessayer
ban-panel-select = Sélectionner le type
ban-panel-server = Ban serveur
ban-panel-role = Ban de rôle
ban-panel-minutes = Minutes
ban-panel-hours = Heures
ban-panel-days = Jours
ban-panel-weeks = Semaines
ban-panel-months = Mois
ban-panel-years = Années
ban-panel-permanent = Permanent
ban-panel-ip-hwid-tooltip = Laissez vide et cochez la case ci-dessous pour utiliser les détails de la dernière connexion
ban-panel-severity = Sévérité :
ban-panel-erase = Effacer les messages de chat et le joueur de la ronde

# Ban string
server-ban-string = {$admin} created a {$severity} severity server ban that expires {$expires} for [{$name}, {$ip}, {$hwid}], with reason: {$reason}
server-ban-string-no-pii = {$admin} created a {$severity} severity server ban that expires {$expires} for {$name} with reason: {$reason}
server-ban-string-never = jamais

# Kick on ban
ban-kick-reason = Vous avez été banni
