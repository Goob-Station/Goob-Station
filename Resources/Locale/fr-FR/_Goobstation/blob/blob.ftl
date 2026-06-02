# SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
# SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
# SPDX-FileCopyrightText: 2024 lanse12 <cloudability.ez@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 GitHubUser53123 <110841413+GitHubUser53123@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
# SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

ent-SpawnPointGhostBlob = Générateur de blob
    .suffix = DEBUG, Générateur de rôle fantôme
    .desc = { ent-MarkerBase.desc }
ent-MobBlobPod = Unité blob
    .desc = Un combattant blob ordinaire.
ent-MobBlobBlobbernaut = Blobbernaut
    .desc = Un combattant blob d'élite.
ent-BaseBlob = blob basique.
    .desc = { "" }
ent-NormalBlobTile = Dalle blob normale
    .desc = Une partie ordinaire du blob nécessaire à la construction de dalles plus avancées.
ent-CoreBlobTile = Noyau du blob
    .desc = L'organe le plus important du blob. En détruisant le noyau, l'infection cessera.
ent-FactoryBlobTile = Usine blob
    .desc = Génère des unités blob et des Blobbernauts au fil du temps.
ent-ResourceBlobTile = Blob de ressources
    .desc = Produit des ressources pour le blob.
ent-NodeBlobTile = Nœud blob
    .desc = Une mini version du noyau qui vous permet de placer des dalles blob spéciales autour de lui.
ent-StrongBlobTile = Dalle blob renforcée
    .desc = Une version renforcée de la dalle normale. Elle ne laisse pas passer l'air et protège contre les dégâts contondants.
ent-ReflectiveBlobTile = Dalles blob réfléchissantes
    .desc = Elles reflètent les lasers, mais protègent moins bien contre les dégâts contondants.
    .desc = { "" }
objective-issuer-blob = Blob


ghost-role-information-blobbernaut-name = Blobbernaut
ghost-role-information-blobbernaut-description = Vous êtes un Blobbernaut. Vous devez défendre le noyau du blob. Utilisez + ou +e dans le chat pour parler dans l'Esprit-blob.

ghost-role-information-blob-name = Blob
ghost-role-information-blob-description = Vous êtes l'infection blob. Consumez la station.

roles-antag-blob-name = Blob
roles-antag-blob-objective = Atteindre une masse critique.

guide-entry-blob = Blob

# Popups
blob-target-normal-blob-invalid = Mauvais type de blob, sélectionnez un blob normal.
blob-target-factory-blob-invalid = Mauvais type de blob, sélectionnez un blob usine.
blob-target-node-blob-invalid = Mauvais type de blob, sélectionnez un blob nœud.
blob-target-close-to-resource = Trop proche d'un autre blob de ressources.
blob-target-nearby-not-node = Aucun nœud ou blob de ressources à proximité.
blob-target-close-to-node = Trop proche d'un autre nœud.
blob-target-already-produce-blobbernaut = Cette usine a déjà produit un Blobbernaut.
blob-cant-split = Vous ne pouvez pas diviser le noyau du blob.
blob-not-have-nodes = Vous n'avez pas de nœuds.
blob-not-enough-resources = Pas assez de ressources.
blob-help = Seul Dieu peut vous aider.
blob-swap-chem = En développement.
blob-mob-attack-blob = Vous ne pouvez pas attaquer un blob.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = Vous mourez en étant hors des dalles blob.
carrier-blob-alert = Il vous reste { $second } secondes avant la transformation.

blob-mob-zombify-second-start = { $pod } commence à vous transformer en zombie.
blob-mob-zombify-third-start = { $pod } commence à transformer { $target } en zombie.

blob-mob-zombify-second-end = { $pod } vous transforme en zombie.
blob-mob-zombify-third-end = { $pod } transforme { $target } en zombie.

blobberaut-factory-destroy = usine détruite
blob-target-already-connected = déjà connecté


# UI
blob-chem-swap-ui-window-name = Changer de produit chimique
blob-chem-reactivespines-info = Épines Réactives
                                Inflige 25 dégâts contondants.
blob-chem-blazingoil-info = Huile Embrasée
                            Inflige 15 dégâts de brûlure et enflamme les cibles.
                            Vous rend vulnérable à l'eau.
blob-chem-regenerativemateria-info = Materia Régénératrice
                                    Inflige 6 dégâts contondants et 15 dégâts de toxines.
                                    Le noyau blob régénère sa santé 10 fois plus vite que la normale et génère 1 ressource supplémentaire.
blob-chem-explosivelattice-info = Réseau Explosif
                                    Inflige 5 dégâts de brûlure et fait exploser la cible, infligeant 10 dégâts contondants.
                                    Les spores explosent à la mort.
                                    Vous devenez immunisé aux explosions.
                                    Vous subissez 50% de dégâts supplémentaires des brûlures et des chocs électriques.
blob-chem-electromagneticweb-info = Toile Électromagnétique
                                    Inflige 20 dégâts de brûlure, 20% de chances de déclencher une impulsion IEM lors d'une attaque.
                                    Les dalles blob déclenchent une impulsion IEM lorsqu'elles sont détruites.
                                    Vous subissez 25% de dégâts contondants et thermiques supplémentaires.

blob-alert-out-off-station = Le blob a été retiré car il était en dehors de la station !

# Announcment
blob-alert-recall-shuttle = La navette d'urgence ne peut pas être envoyée tant qu'un biohazard de niveau 5 est présent sur la station.
blob-alert-detect = Épidémie confirmée de biohazard de niveau 5 à bord de la station. Tout le personnel doit contenir l'épidémie.
blob-alert-critical = Niveau de biohazard critique, les codes d'authentification nucléaire ont été envoyés à la station. Le Commandement Central ordonne au personnel restant d'activer le mécanisme d'autodestruction.
blob-alert-critical-NoNukeCode = Niveau de biohazard critique. Le Commandement Central ordonne au personnel restant de se mettre à l'abri et d'attendre les secours.
blob-alert-shuttle-arrived = Biohazard détecté à bord. Tout le personnel doit évacuer immédiatement.

# Actions
blob-teleport-to-node-action-name = Sauter vers un Nœud (0)
blob-teleport-to-node-action-desc = Vous téléporte vers un nœud blob aléatoire.
blob-help-action-name = Aide
blob-help-action-desc = Obtenir des informations de base sur le jeu en tant que blob.

# Ghost role
blob-carrier-role-name = Porteur de blob
blob-carrier-role-desc = Une créature infectée par un blob.
blob-carrier-role-rules = Vous êtes un antagoniste. Vous avez 10 minutes avant de vous transformer en blob.
                        Profitez de ce temps pour trouver un endroit sûr sur la station. Gardez à l'esprit que vous serez très faible juste après la transformation.
blob-carrier-role-greeting = Vous êtes porteur du Blob. Trouvez un endroit isolé sur la station et transformez-vous en Blob. Transformez la station en masse et ses habitants en vos serviteurs. Nous sommes tous des Blobs.

# Verbs
blob-pod-verb-zombify = Zombifier
blob-verb-upgrade-to-strong = Améliorer en Blob Renforcé
blob-verb-upgrade-to-reflective = Améliorer en Blob Réfléchissant
blob-verb-remove-blob-tile = Retirer la dalle blob

# Alerts
blob-resource-alert-name = Ressources du Noyau
blob-resource-alert-desc = Vos ressources produites par le noyau et les blobs de ressources. Utilisez-les pour vous étendre et créer des blobs spéciaux.
blob-health-alert-name = Santé du Noyau
blob-health-alert-desc = La santé de votre noyau. Vous mourrez si elle atteint zéro.

# Greeting
blob-role-greeting =
    Vous êtes un blob — une créature spatiale parasite capable de détruire des stations entières.
        Votre objectif est de survivre et de vous développer le plus possible.
        Vous êtes presque invulnérable aux dégâts physiques, mais la chaleur peut encore vous blesser.
        Utilisez Alt+Clic gauche pour améliorer les dalles blob normales en dalles renforcées, et les renforcées en réfléchissantes.
        Pensez à placer des blobs de ressources pour générer des ressources.
        Gardez à l'esprit que les blobs de ressources et les usines ne fonctionneront qu'à proximité des nœuds blob ou des noyaux.
        Vous pouvez utiliser + ou +e dans le chat pour utiliser l'Esprit-blob afin de parler à vos sbires.
blob-zombie-greeting = Vous avez été infecté et ressuscité par une spore blob. Vous devez maintenant aider le blob à prendre le contrôle de la station. Utilisez +e dans le chat pour parler dans l'Esprit-blob.

# End round
blob-round-end-result =
    { $blobCount ->
        [one] Il y avait une infection blob.
        *[other] Il y avait {$blobCount} blobs.
    }

blob-user-was-a-blob = [color=gray]{$user}[/color] était un blob.
blob-user-was-a-blob-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) était un blob.
blob-was-a-blob-named = [color=White]{$name}[/color] était un blob.

preset-blob-objective-issuer-blob = [color=#33cc00]Blob[/color]

blob-user-was-a-blob-with-objectives = [color=gray]{$user}[/color] était un blob qui avait les objectifs suivants :
blob-user-was-a-blob-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) était un blob qui avait les objectifs suivants :
blob-was-a-blob-with-objectives-named = [color=White]{$name}[/color] était un blob qui avait les objectifs suivants :

# Objectivies
objective-condition-blob-capture-title = Prendre le contrôle de la station
objective-condition-blob-capture-description = Votre seul objectif est de prendre le contrôle de toute la station. Vous devez avoir au moins {$count} dalles blob.
objective-condition-success = { $condition } | [color={ $markupColor }]Réussi ![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Échoué ![/color] ({ $progress }%)

# Admin Verbs

admin-verb-make-blob = Make the target into a blob carrier.
admin-verb-text-make-blob = Make Blob Carrier

# Language
language-Blob-name = Blob
chat-language-Blob-name = Blob
language-Blob-description = Bleeb bob ! Blob blob !