# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Mervill <mervills.email@gmail.com>
# SPDX-FileCopyrightText: 2023 alexkar598 <25136265+alexkar598@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Repo <47093363+Titian3@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Affiché comme initiateur du vote quand aucun utilisateur ne crée le vote
ui-vote-initiator-server = Le serveur

## Default.Votes
ui-vote-restart-title = Redémarrer le round
ui-vote-restart-succeeded = Le vote de redémarrage a réussi.
ui-vote-restart-failed = Le vote de redémarrage a échoué (nécessite { TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Vote de redémarrage échoué : un minimum de { $ghostPlayerRequirement }% de joueurs fantômes est requis pour initier un vote de redémarrage. Actuellement, il n'y a pas assez de joueurs fantômes.
ui-vote-restart-yes = Oui
ui-vote-restart-no = Non
ui-vote-restart-abstain = Abstention
ui-vote-gamemode-title = Prochain mode de jeu
ui-vote-gamemode-tie = Égalité pour le vote du mode de jeu ! Sélection en cours... { $picked }
ui-vote-gamemode-win = { $winner } a remporté le vote du mode de jeu !
ui-vote-map-title = Prochaine carte
ui-vote-map-tie = Égalité pour le vote de la carte ! Sélection en cours... { $picked }
ui-vote-map-win = { $winner } a remporté le vote de la carte !
ui-vote-map-notlobby = Le vote pour les cartes n'est valide que dans le lobby pré-round !
ui-vote-map-notlobby-time = Le vote pour les cartes n'est valide que dans le lobby pré-round avec { $time } restant !
# Votes de votekick
ui-vote-votekick-unknown-initiator = Un joueur
ui-vote-votekick-unknown-target = Joueur inconnu
ui-vote-votekick-title = { $initiator } a lancé un votekick pour l'utilisateur : { $targetEntity }. Raison : { $reason }
ui-vote-votekick-yes = Oui
ui-vote-votekick-no = Non
ui-vote-votekick-abstain = Abstention
ui-vote-votekick-success = Le votekick de { $target } a réussi. Raison : { $reason }
ui-vote-votekick-failure = Le votekick de { $target } a échoué. Raison : { $reason }
ui-vote-votekick-not-enough-eligible = Pas assez de votants éligibles en ligne pour lancer un votekick : { $voters }/{ $requirement }
ui-vote-votekick-server-cancelled = Le votekick de { $target } a été annulé par le serveur.