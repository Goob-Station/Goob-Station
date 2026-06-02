# SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
# SPDX-FileCopyrightText: 2023 James Simonson <jamessimo89@gmail.com>
# SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

anomaly-component-contact-damage = L'anomalie vous brûle la peau !

anomaly-vessel-component-anomaly-assigned = Anomalie assignée au réceptacle.
anomaly-vessel-component-not-assigned = Ce réceptacle n'est assigné à aucune anomalie. Essayez d'utiliser un scanner dessus.
anomaly-vessel-component-assigned = Ce réceptacle est actuellement assigné à une anomalie.

anomaly-particles-delta = Particules Delta
anomaly-particles-epsilon = Particules Epsilon
anomaly-particles-zeta = Particules Zeta
anomaly-particles-omega = Particules Omega
anomaly-particles-sigma = Particules Sigma

anomaly-scanner-component-scan-complete = Analyse terminée !

anomaly-scanner-ui-title = scanner d'anomalie
anomaly-scanner-no-anomaly = Aucune anomalie actuellement scannée.
anomaly-scanner-severity-percentage = Sévérité actuelle : [color=gray]{$percent}[/color]
anomaly-scanner-severity-percentage-unknown = Sévérité actuelle : [color=red]ERREUR[/color]
anomaly-scanner-stability-low = État actuel : [color=gold]En déclin[/color]
anomaly-scanner-stability-medium = État actuel : [color=forestgreen]Stable[/color]
anomaly-scanner-stability-high = État actuel : [color=crimson]En croissance[/color]
anomaly-scanner-stability-unknown = État actuel : [color=red]ERREUR[/color]
anomaly-scanner-point-output = Production de points : [color=gray]{$point}[/color]
anomaly-scanner-point-output-unknown = Production de points : [color=red]ERREUR[/color]
anomaly-scanner-particle-readout = Analyse de réaction particulaire :
anomaly-scanner-particle-danger = - [color=crimson]Type dangereux :[/color] {$type}
anomaly-scanner-particle-unstable = - [color=plum]Type instable :[/color] {$type}
anomaly-scanner-particle-containment = - [color=goldenrod]Type de confinement :[/color] {$type}
anomaly-scanner-particle-transformation = - [color=#6b75fa]Type de transformation :[/color] {$type}
anomaly-scanner-particle-danger-unknown = - [color=crimson]Type dangereux :[/color] [color=red]ERREUR[/color]
anomaly-scanner-particle-unstable-unknown = - [color=plum]Type instable :[/color] [color=red]ERREUR[/color]
anomaly-scanner-particle-containment-unknown = - [color=goldenrod]Type de confinement :[/color] [color=red]ERREUR[/color]
anomaly-scanner-particle-transformation-unknown = - [color=#6b75fa]Type de transformation :[/color] [color=red]ERREUR[/color]
anomaly-scanner-pulse-timer = Temps avant la prochaine pulsation : [color=gray]{$time}[/color]

anomaly-gorilla-core-slot-name = Noyau d'anomalie
anomaly-gorilla-charge-none = Il ne contient pas de [bold]noyau d'anomalie[/bold].
anomaly-gorilla-charge-limit = Il lui reste [color={$count ->
    [3]green
    [2]yellow
    [1]orange
    [0]red
    *[other]purple
}]{$count} {$count ->
    [one]charge
    *[other]charges
}[/color].
anomaly-gorilla-charge-infinite = Il possède [color=gold]des charges infinies[/color]. [italic]Pour l'instant...[/italic]

anomaly-sync-connected = Anomalie attachée avec succès
anomaly-sync-disconnected = La connexion à l'anomalie a été perdue !
anomaly-sync-no-anomaly = Aucune anomalie à portée.
anomaly-sync-examine-connected = Il est [color=darkgreen]attaché[/color] à une anomalie.
anomaly-sync-examine-not-connected = Il n'est [color=darkred]pas attaché[/color] à une anomalie.
anomaly-sync-connect-verb-text = Attacher l'anomalie
anomaly-sync-connect-verb-message = Attacher une anomalie proche à {THE($machine)}.
anomaly-sync-disconnect-verb-text = Détacher l'anomalie
anomaly-sync-disconnect-verb-message = Détacher l'anomalie connectée de {THE($machine)}.

anomaly-generator-ui-title = Générateur d'Anomalie
anomaly-generator-fuel-display = Carburant :
anomaly-generator-cooldown = Recharge : [color=gray]{$time}[/color]
anomaly-generator-no-cooldown = Recharge : [color=gray]Terminée[/color]
anomaly-generator-yes-fire = Statut : [color=forestgreen]Prêt[/color]
anomaly-generator-no-fire = Statut : [color=crimson]Pas prêt[/color]
anomaly-generator-generate = Générer une Anomalie
anomaly-generator-charges = {$charges ->
    [one] {$charges} charge
    *[other] {$charges} charges
}
anomaly-generator-announcement = Une anomalie a été générée !

anomaly-command-pulse = Pulse une anomalie cible
anomaly-command-supercritical = Rend une anomalie cible supercritique

# Texte de pied de page
anomaly-generator-flavor-left = L'anomalie peut apparaître dans l'opérateur.
anomaly-generator-flavor-right = v1.1

anomaly-behavior-unknown = [color=red]ERREUR. Impossible de lire.[/color]

anomaly-behavior-title = analyse des déviations comportementales :
anomaly-behavior-point = [color=gold]L'anomalie produit {$mod}% des points[/color]

anomaly-behavior-safe = [color=forestgreen]L'anomalie est extrêmement stable. Pulsations extrêmement rares.[/color]
anomaly-behavior-slow = [color=forestgreen]La fréquence des pulsations est bien moins élevée.[/color]
anomaly-behavior-light = [color=forestgreen]La puissance des pulsations est significativement réduite.[/color]
anomaly-behavior-balanced = Aucune déviation comportementale détectée.
anomaly-behavior-delayed-force = La fréquence des pulsations est fortement réduite, mais leur puissance est augmentée.
anomaly-behavior-rapid = La fréquence des pulsations est bien plus élevée, mais leur intensité est atténuée.
anomaly-behavior-reflect = Un revêtement protecteur a été détecté.
anomaly-behavior-nonsensivity = Une réaction faible aux particules a été détectée.
anomaly-behavior-sensivity = Une réaction amplifiée aux particules a été détectée.
anomaly-behavior-invisibility = Une distorsion des ondes lumineuses a été détectée.
anomaly-behavior-secret = Interférence détectée. Certaines données ne peuvent être lues.
anomaly-behavior-inconstancy = [color=crimson]Une impermanence a été détectée. Les types de particules peuvent changer avec le temps.[/color]
anomaly-behavior-fast = [color=crimson]La fréquence de pulsation est fortement augmentée.[/color]
anomaly-behavior-strenght = [color=crimson]La puissance de pulsation est significativement augmentée.[/color]
anomaly-behavior-moving = [color=crimson]Une instabilité de coordonnées a été détectée.[/color]
