# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Morbo <exstrominer@gmail.com>
# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Interrobang01 <113810873+Interrobang01@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ZeroDayDaemon <60460608+ZeroDayDaemon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Arkanic <50847107+Arkanic@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Traître

traitor-round-end-codewords = Les noms de code étaient : [color=White]{$codewords}[/color]
traitor-round-end-agent-name = un traître

objective-issuer-syndicate = [color=crimson]Le Syndicat[/color]
objective-issuer-unknown = Inconnu

# Shown at the end of a round of Traitor

traitor-title = Traître
traitor-description = Il y a des traîtres parmi nous...
traitor-not-enough-ready-players = Pas assez de joueurs prêts pour la partie ! Il y avait {$readyPlayersCount} joueurs prêts sur les {$minimumPlayers} requis. Impossible de démarrer Traître.
traitor-no-one-ready = Aucun joueur n'est prêt ! Impossible de démarrer Traitor.

## TraitorDeathMatch
traitor-death-match-title = Traitor Deathmatch
traitor-death-match-description = Tout le monde est un traître. Tout le monde veut la mort des autres.
traitor-death-match-station-is-too-unsafe-announcement = La station est trop dangereuse pour continuer. Il vous reste une minute.
traitor-death-match-end-round-description-first-line = Les PDA récupérés après coup...
traitor-death-match-end-round-description-entry = Le PDA de {$originalName}, avec {$tcBalance} TC

## TraitorRole

# TraitorRole
traitor-role-greeting =
    Vous êtes un agent envoyé par {$corporation} au nom du [color = darkred]Syndicat.[/color]
    Vos objectifs et noms de code sont répertoriés dans le menu du personnage.
    Utilisez votre uplink pour acheter les outils dont vous aurez besoin pour cette mission.
    Mort à Nanotrasen !
traitor-role-codewords =
    Les noms de code sont : [color = lightgray]
    {$codewords}.[/color]
    Les noms de code peuvent être utilisés dans une conversation ordinaire pour s'identifier discrètement auprès d'autres traîtres.
traitor-role-uplink-code =
    Le code de votre uplink est [color=white]{$code}[/color]. Définissez-la comme sonnerie de votre PDA pour accéder au marché noir.
    N'oublie pas de le fermer après, sinon l'équipage de la station pourra facilement l'ouvrir lui aussi !
traitor-role-uplink-pen-code =
    Faites tourner votre stylo selon la combinaison [color = lightgray]{$code}[/color] pour déverrouiller votre uplink.
    Les degrés correspondent aux angles de rotation. L'uplink se verrouille automatiquement lorsqu'il est fermé.
traitor-role-uplink-implant =
    Votre implant uplink a été activé, accédez-y depuis votre barre de raccourcis.
    L'uplink est sécurisé tant qu'il n'est pas retiré de votre corps.

# don't need all the flavour text for character menu
traitor-role-codewords-short =
    Les noms de code sont :
    {$codewords}.

traitor-role-uplink-code-short = Votre code uplink est {$code}. Définissez-le comme sonnerie de votre PDA pour accéder au marché noir.
traitor-role-uplink-pen-code-short = Votre code d'uplink par stylo est {$code}. Tournez le stylo pour le déverrouiller. Il se verrouille lorsqu'il est fermé.
traitor-role-uplink-implant-short = Votre uplink a été implantée. Accédez-y depuis votre barre de raccourcis.

traitor-role-moreinfo =
    Trouvez plus d'informations sur votre rôle dans le menu du personnage.

traitor-role-nouplink =
    Malheureusement, vous n'avez pas accès au marché noir.

traitor-role-allegiances =
    Vos allégeances :

traitor-role-notes =
    Notes supplémentaires :

