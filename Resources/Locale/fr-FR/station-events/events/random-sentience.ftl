# SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nim <128169402+Nimfar11@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 deathride58 <deathride58@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Formules utilisées pour indiquer la source des informations du Commandement Central.
random-sentience-event-data-1 = les relevés de nos capteurs longue portée
random-sentience-event-data-2 = nos modèles probabilistes sophistiqués
random-sentience-event-data-3 = notre omniscience
random-sentience-event-data-4 = le trafic de communications de votre station
random-sentience-event-data-5 = les émissions d'énergie que nous avons détectées
random-sentience-event-data-6 = [CENSURÉ]

## Formules utilisées pour décrire le niveau d'intelligence, bien que cela n'ait aucun effet réel.
random-sentience-event-strength-1 = humain
random-sentience-event-strength-2 = primate
random-sentience-event-strength-3 = modéré
random-sentience-event-strength-4 = sécurité
random-sentience-event-strength-5 = commandement
random-sentience-event-strength-6 = clown
random-sentience-event-strength-7 = faible
random-sentience-event-strength-8 = IA

## Texte d'annonce

station-event-random-sentience-announcement = Sur la base de { $data }, nous estimons que certains des êtres { $amount ->
    [1] { $kind1 }
    [2] { $kind1 } et { $kind2 }
    [3] { $kind1 }, { $kind2 } et { $kind3 }
    *[other] { $kind1 }, { $kind2 }, { $kind3 }, etc.
} de la station ont développé une intelligence de niveau { $strength }, ainsi que la capacité de communiquer.

## Description du rôle fantôme

station-event-random-sentience-role-description = Vous êtes un(e) { $name } sensible, ramené(e) à la vie par la magie spatiale.

# Catégories
station-event-random-sentience-flavor-mechanical = mécanique
station-event-random-sentience-flavor-organic = organique
station-event-random-sentience-flavor-corgi = corgi
station-event-random-sentience-flavor-primate = primate
station-event-random-sentience-flavor-kobold = kobold
station-event-random-sentience-flavor-slime = slime
station-event-random-sentience-flavor-inanimate = inanimé
station-event-random-sentience-flavor-scurret = scurret
