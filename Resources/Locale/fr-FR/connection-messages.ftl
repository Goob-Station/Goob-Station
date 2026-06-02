# SPDX-FileCopyrightText: 2022 Dylan Corrales <DeathCamel58@gmail.com>
# SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Myra <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2025 PJB3005 <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-whitelistadd-desc = Adds the player with the given username to the server whitelist.
cmd-whitelistadd-help = Usage: whitelistadd <username or User ID>
cmd-whitelistadd-existing = {$username} is already on the whitelist!
cmd-whitelistadd-added = {$username} added to the whitelist
cmd-whitelistadd-not-found = Unable to find '{$username}'
cmd-whitelistadd-arg-player = [player]

cmd-whitelistremove-desc = Removes the player with the given username from the server whitelist.
cmd-whitelistremove-help = Usage: whitelistremove <username or User ID>
cmd-whitelistremove-existing = {$username} is not on the whitelist!
cmd-whitelistremove-removed = {$username} removed from the whitelist
cmd-whitelistremove-not-found = Unable to find '{$username}'
cmd-whitelistremove-arg-player = [player]

cmd-kicknonwhitelisted-desc = Kicks all non-whitelisted players from the server.
cmd-kicknonwhitelisted-help = Usage: kicknonwhitelisted

ban-banned-permanent = Ce bannissement ne sera levé que par appel.
ban-banned-permanent-appeal = Ce bannissement ne sera levé que par appel. Vous pouvez faire appel sur {$link}
ban-expires = Ce bannissement dure {$duration} minutes et expirera le {$time} UTC.
ban-banned-1 = Vous, ou un autre utilisateur de cet ordinateur ou de cette connexion, êtes banni de jouer ici.
ban-banned-2 = Vous avez été banni par : "{$adminName}"
ban-banned-3 = La raison du bannissement est : "{$reason}"
ban-banned-4 = Toute tentative de contournement de ce bannissement, comme la création d'un nouveau compte, sera enregistrée.

soft-player-cap-full = Le serveur est plein !
panic-bunker-account-denied = Ce serveur est en mode bunker anti-raid, souvent activé en prévention des raids. Les nouvelles connexions de comptes ne remplissant pas certaines conditions sont temporairement refusées. Réessayez plus tard.
panic-bunker-account-denied-reason = Ce serveur est en mode bunker anti-raid, souvent activé en prévention des raids. Les nouvelles connexions de comptes ne remplissant pas certaines conditions sont temporairement refusées. Réessayez plus tard. Raison : "{$reason}"
panic-bunker-account-reason-account = Votre compte Space Station 14 est trop récent. Il doit avoir plus de {$minutes} minutes.
panic-bunker-account-reason-overall = Votre temps de jeu total sur le serveur doit être supérieur à {$minutes} $minutes.

whitelist-playtime = Vous n'avez pas suffisamment de temps de jeu pour rejoindre ce serveur. Vous avez besoin d'au moins {$minutes} minutes de jeu.
whitelist-player-count = Ce serveur n'accepte pas de joueurs pour l'instant. Réessayez plus tard.
whitelist-notes = Vous avez actuellement trop de notes d'administration pour rejoindre ce serveur. Vous pouvez consulter vos notes en tapant /adminremarks dans le chat.
whitelist-manual = Vous n'êtes pas sur la liste blanche de ce serveur.
whitelist-blacklisted = Vous êtes sur la liste noire de ce serveur.
whitelist-always-deny = Vous n'êtes pas autorisé à rejoindre ce serveur.
whitelist-fail-prefix = Non autorisé : {$msg}

cmd-blacklistadd-desc = Adds the player with the given username to the server blacklist.
cmd-blacklistadd-help = Usage: blacklistadd <username>
cmd-blacklistadd-existing = {$username} is already on the blacklist!
cmd-blacklistadd-added = {$username} added to the blacklist
cmd-blacklistadd-not-found = Unable to find '{$username}'
cmd-blacklistadd-arg-player = [player]

cmd-blacklistremove-desc = Removes the player with the given username from the server blacklist.
cmd-blacklistremove-help = Usage: blacklistremove <username>
cmd-blacklistremove-existing = {$username} is not on the blacklist!
cmd-blacklistremove-removed = {$username} removed from the blacklist
cmd-blacklistremove-not-found = Unable to find '{$username}'
cmd-blacklistremove-arg-player = [player]

baby-jail-account-denied = Ce serveur est un serveur pour débutants, destiné aux nouveaux joueurs et à ceux qui veulent les aider. Les nouvelles connexions de comptes trop anciens ou non listés en liste blanche ne sont pas acceptées. Découvrez d'autres serveurs et tout ce que Space Station 14 a à offrir. Amusez-vous bien !
baby-jail-account-denied-reason = Ce serveur est un serveur pour débutants, destiné aux nouveaux joueurs et à ceux qui veulent les aider. Les nouvelles connexions de comptes trop anciens ou non listés ne sont pas acceptées. Découvrez d'autres serveurs et tout ce que Space Station 14 a à offrir. Amusez-vous bien ! Raison : "{$reason}"
baby-jail-account-reason-account = Votre compte Space Station 14 est trop ancien. Il doit avoir moins de {$minutes} minutes.
baby-jail-account-reason-overall = Votre temps de jeu total sur le serveur doit être inférieur à {$minutes} $minutes.

generic-misconfigured = Le serveur est mal configuré et n'accepte pas de joueurs. Veuillez contacter le propriétaire du serveur et réessayer plus tard.

ipintel-server-ratelimited = Ce serveur utilise un système de sécurité avec vérification externe, qui a atteint sa limite maximale de vérifications. Veuillez contacter l'équipe d'administration du serveur et réessayer plus tard.
ipintel-unknown = Ce serveur utilise un système de sécurité avec vérification externe, mais une erreur s'est produite. Veuillez contacter l'équipe d'administration du serveur et réessayer plus tard.
ipintel-suspicious = Vous semblez vous connecter via un datacenter ou un VPN. Pour des raisons administratives, les connexions VPN ne sont pas autorisées. Veuillez contacter l'équipe d'administration si vous pensez qu'il s'agit d'une erreur.

hwid-required = Votre client a refusé d'envoyer un identifiant matériel. Veuillez contacter l'équipe d'administration pour obtenir de l'aide.
