# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Myctai <108953437+Myctai@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Radosvik <65792927+Radosvik@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ZeroDayDaemon <60460608+ZeroDayDaemon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

game-ticker-restart-round = Redémarrage de la partie...
game-ticker-start-round = TLa partie démarre maintenant...
game-ticker-start-round-cannot-start-game-mode-fallback = Échec du démarrage du mode de jeu {$failedGameMode} ! Valeur par défaut : {$fallbackMode}...
game-ticker-start-round-cannot-start-game-mode-restart = Échec du démarrage du mode de jeu {$failedGameMode} ! Redémarrage de la partie...
game-ticker-start-round-invalid-map = La carte sélectionnée {$map} est inéligible pour le mode de jeu {$mode}. Le mode de jeu  pourrait ne pas fonctionner comme prévu...
game-ticker-unknown-role = Inconnu
game-ticker-delay-start = Le début de la partie est retardée de {$seconds} secondes.
game-ticker-pause-start = Le compte à rebours du début du round a été mis sur pause.
game-ticker-pause-start-resumed = Le compte à rebours du début du round a repris.
game-ticker-player-join-game-message = Bienvenue sur Space Station 14 ! Si c'est la première fois que vous jouez, pensez à lire les règles du jeu, et n'hésitez pas à demander de l'aide en LOOC (local OOC) ou OOC (généralement seulement disponible entre les parties).
game-ticker-get-info-text = Bonjour et bienvenue sur [color=white]Space Station 14 ![/color]
                            La partie actuelle est : [color=white]#{$roundId}[/color]
                            Le nombre de joueurs actuel est : [color=white]{$playerCount}[/color]
                            La carte actuelle est : [color=white]{$mapName}[/color]
                            Le mode de jeu actuel est : [color=white]{$gmTitle}[/color]
                            >[color=yellow]{$desc}[/color]
game-ticker-get-info-preround-text = Bonjour et bienvenue sur [color=white]Space Station 14 ![/color]
                            La partie actuelle est : [color=white]#{$roundId}[/color]
                            Le nombre de joueurs actuel est : [color=white]{$playerCount}[/color] ([color=white]{$readyCount}[/color] {$readyCount ->
                                [one] est
                                *[other] sont
                            } prêt)
                            La carte actuelle est : [color=white]{$mapName}[/color]
                            Le mode de jeu actuel est : [color=white]{$gmTitle}[/color]
                            >[color=yellow]{$desc}[/color]
game-ticker-no-map-selected = [color=yellow]Carte pas encore sélectionnée ![/color]
game-ticker-player-no-jobs-available-when-joining = Quand vous avez essayé de rejoindre la partie, aucun rôle n'était disponible.

# Displayed in chat to admins when a player joins
player-join-message = Le joueur {$name} a rejoint.
player-first-join-message = Le joueur {$name} a rejoint pour la première fois.

# Displayed in chat to admins when a player leaves
player-leave-message = Le joueur {$name} est parti.

latejoin-arrival-announcement = {$character} ({$job}) est arrivé à la station !
latejoin-arrival-announcement-special = {$job} {$character} est prêt !
latejoin-arrival-sender = Station
latejoin-arrivals-direction = Une navette de transfert vers votre station arrivera bientôt.
latejoin-arrivals-direction-time = Une navette de transfert vers votre station arrivera dans {$time}.
latejoin-arrivals-dumped-from-shuttle = Une force mystérieuse vous empêche de partir avec la navette des arrivées.
latejoin-arrivals-teleport-to-spawn = Une force mystérieuse vous téléporte hors de la navette des arrivées. Passez un bon service !

preset-not-enough-ready-players = Ne peut pas démarrer {$presetName}. Il faut au moins {$minimumPlayers} joueurs mais il n'y en a que {$readyPlayersCount}.
preset-no-one-ready = Ne peut pas démarrer {$presetName}. Aucun joueur n'est prêt.

game-run-level-PreRoundLobby = Lobby pré-partie
game-run-level-InRound = En partie
game-run-level-PostRound = Post partie
