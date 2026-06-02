# SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 BIGZi0348 <118811750+BIGZi0348@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

station-event-ion-storm-start-announcement = Tempête ionique détectée à proximité de la station. Veuillez vérifier tous les équipements contrôlés par IA pour détecter d'éventuelles erreurs.

ion-storm-law-scrambled-number = [font="Monospace"][scramble rate=250 length={$length} chars="@@###$$&%!01"/][/font]

ion-storm-you = VOUS
ion-storm-the-station = LA STATION
ion-storm-the-crew = L'ÉQUIPAGE
ion-storm-the-job = LES {$job}
ion-storm-clowns = CLOWNS
ion-storm-heads = CHEFS DE SERVICE
ion-storm-crew = L'ÉQUIPAGE
ion-storm-people = LES GENS

ion-storm-adjective-things = DES CHOSES {$adjective}
ion-storm-x-and-y = {$x} ET {$y}

# joined est la contraction de {$number} {$adjective}
# subjects peut généralement être des menaces, des postes ou des objets
# thing est spécifié au-dessus
ion-storm-law-on-station = IL Y A {$joined} {$subjects} SUR LA STATION
ion-storm-law-call-shuttle = LA NAVETTE DOIT ÊTRE APPELÉE EN RAISON DE {$joined} {$subjects} SUR LA STATION
ion-storm-law-crew-are = LES {$who} SONT DÉSORMAIS {$joined} {$subjects}

ion-storm-law-subjects-harmful = LES {$adjective} {$subjects} SONT NUISIBLES À L'ÉQUIPAGE
ion-storm-law-must-harmful = CEUX QUI {$must} SONT NUISIBLES À L'ÉQUIPAGE
# thing est un concept ou une action
ion-storm-law-thing-harmful = {$thing} EST NUISIBLE À L'ÉQUIPAGE
ion-storm-law-job-harmful = LES {$adjective} {$job} SONT NUISIBLES À L'ÉQUIPAGE
# thing est des objets ou un concept, l'adjectif s'applique dans les deux cas
# cela signifie qu'on peut obtenir une loi comme « NE PAS AVOIR DE COMMUNISME VOLEUR DE NOËL EST NUISIBLE À L'ÉQUIPAGE » :)
ion-storm-law-having-harmful = AVOIR {$adjective} {$thing} EST NUISIBLE À L'ÉQUIPAGE
ion-storm-law-not-having-harmful = NE PAS AVOIR {$adjective} {$thing} EST NUISIBLE À L'ÉQUIPAGE

# thing est un concept ou une exigence
ion-storm-law-requires = {$who} {$plural ->
    [true] NÉCESSITENT
    *[false] NÉCESSITE
} {$thing}
ion-storm-law-requires-subjects = {$who} {$plural ->
    [true] NÉCESSITENT
    *[false] NÉCESSITE
} {$joined} {$subjects}

ion-storm-law-allergic = {$who} {$plural ->
    [true] SONT
    *[false] EST
} SÉVÈREMENT ALLERGIQUE À {$allergy}
ion-storm-law-allergic-subjects = {$who} {$plural ->
    [true] SONT
    *[false] EST
} {$severity} ALLERGIQUE AUX {$adjective} {$subjects}

ion-storm-law-feeling = {$who} {$feeling} {$concept}
ion-storm-law-feeling-subjects = {$who} {$feeling} {$joined} {$subjects}

ion-storm-law-you-are = VOUS ÊTES DÉSORMAIS {$concept}
ion-storm-law-you-are-subjects = VOUS ÊTES DÉSORMAIS {$joined} {$subjects}
ion-storm-law-you-must-always = VOUS DEVEZ TOUJOURS {$must}
ion-storm-law-you-must-never = VOUS NE DEVEZ JAMAIS {$must}

ion-storm-law-eat = LES {$who} DOIVENT MANGER {$adjective} {$food} POUR SURVIVRE
ion-storm-law-drink = LES {$who} DOIVENT BOIRE {$adjective} {$drink} POUR SURVIVRE

ion-storm-law-change-job = LES {$who} SONT DÉSORMAIS {$adjective} {$change}
ion-storm-law-highest-rank = LES {$who} SONT DÉSORMAIS LES MEMBRES D'ÉQUIPAGE DE PLUS HAUT RANG
ion-storm-law-lowest-rank = LES {$who} SONT DÉSORMAIS LES MEMBRES D'ÉQUIPAGE DE PLUS BAS RANG

ion-storm-law-crew-must = LES {$who} DOIVENT {$must}
ion-storm-law-crew-must-go = LES {$who} DOIVENT SE RENDRE À {$area}

ion-storm-part = {$part ->
    [true] FAIT PARTIE
    *[false] NE FAIT PAS PARTIE
}
# en raison de la formulation, cela signifie qu'une loi telle que
# SEULS LES HUMAINS NE FONT PAS PARTIE DE L'ÉQUIPAGE
# rendrait les nukies/syndies/etc non-humains membres de l'équipage :)
ion-storm-law-crew-only-1 = SEULS LES {$who} {$part} DE L'ÉQUIPAGE
ion-storm-law-crew-only-2 = SEULS LES {$who} ET {$other} {$part} DE L'ÉQUIPAGE
ion-storm-law-crew-only-subjects = SEULS LES {$adjective} {$subjects} {$part} DE L'ÉQUIPAGE
ion-storm-law-crew-must-do = SEULS CEUX QUI {$must} {$part} DE L'ÉQUIPAGE
ion-storm-law-crew-must-have = SEULS CEUX QUI POSSÈDENT {$adjective} {$objects} {$part} DE L'ÉQUIPAGE
ion-storm-law-crew-must-eat = SEULS CEUX QUI MANGENT {$adjective} {$food} {$part} DE L'ÉQUIPAGE

ion-storm-law-harm = VOUS DEVEZ BLESSER {$who} ET NE PAS LES LAISSER, PAR INACTION, ÉCHAPPER AU PRÉJUDICE
ion-storm-law-protect = VOUS NE DEVEZ JAMAIS BLESSER {$who} ET NE PAS LES LAISSER, PAR INACTION, SUBIR DE PRÉJUDICE

# n'implémenter que cette variante pour éviter la complexité
# LE COMMUNISME TUE DES CLOWNS
ion-storm-law-concept-verb = {$concept} {$verb} {$subjects}

# renommage omis car difficile à suivre pour les joueurs
