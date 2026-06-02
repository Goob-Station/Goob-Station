# SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Sailor <109166122+Equivocateur@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 mhamster <81412348+mhamsterr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 Eris <eris@erisws.com>
# SPDX-FileCopyrightText: 2024 Flesh <62557990+PolterTzi@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Gotimanga <127038462+Gotimanga@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
# SPDX-FileCopyrightText: 2024 Zonespace <41448081+Zonespace27@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 alex-georgeff <54858069+taurie@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

-create-3rd-person =
    { $chance ->
        [1] Crée
        *[other] créer
    }

-cause-3rd-person =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    }

-satiate-3rd-person =
    { $chance ->
        [1] Rassasie
        *[other] rassasier
    }

reagent-effect-guidebook-create-entity-reaction-effect =
    { $chance ->
        [1] Crée
        *[other] créer
    } { $amount ->
        [1] {INDEFINITE($entname)}
        *[other] {$amount} {MAKEPLURAL($entname)}
    }

reagent-effect-guidebook-explosion-reaction-effect =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } une explosion

reagent-effect-guidebook-emp-reaction-effect =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } une impulsion électromagnétique

reagent-effect-guidebook-flash-reaction-effect =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } un éclair aveuglant

reagent-effect-guidebook-foam-area-reaction-effect =
    { $chance ->
        [1] Crée
        *[other] créer
    } de grandes quantités de mousse

reagent-effect-guidebook-smoke-area-reaction-effect =
    { $chance ->
        [1] Crée
        *[other] créer
    } de grandes quantités de fumée

reagent-effect-guidebook-satiate-thirst =
    { $chance ->
        [1] Rassasie
        *[other] rassasier
    } { $relative ->
        [1] la soif normalement
        *[other] la soif à {NATURALFIXED($relative, 3)}x le taux moyen
    }

reagent-effect-guidebook-satiate-hunger =
    { $chance ->
        [1] Rassasie
        *[other] rassasier
    } { $relative ->
        [1] la faim normalement
        *[other] la faim à {NATURALFIXED($relative, 3)}x le taux moyen
    }

reagent-effect-guidebook-health-change =
    { $chance ->
        [1] { $healsordeals ->
                [heals] Soigne
                [deals] Inflige
                *[both] Modifie la santé de
             }
        *[other] { $healsordeals ->
                    [heals] soigner
                    [deals] infliger
                    *[both] modifier la santé de
                 }
    } { $changes }

reagent-effect-guidebook-even-health-change =
    { $chance ->
        [1] { $healsordeals ->
            [heals] Soigne uniformément
            [deals] Inflige uniformément
            *[both] Modifie uniformément la santé de
        }
        *[other] { $healsordeals ->
            [heals] soigner uniformément
            [deals] infliger uniformément
            *[both] modifier uniformément la santé de
        }
    } { $changes }

reagent-effect-guidebook-status-effect =
    { $type ->
        [add]   { $chance ->
                    [1] Provoque
                    *[other] provoquer
                } {LOC($key)} pendant au moins {NATURALFIXED($time, 3)} {MANY("seconde", $time)} avec accumulation
        *[set]  { $chance ->
                    [1] Provoque
                    *[other] provoquer
                } {LOC($key)} pendant au moins {NATURALFIXED($time, 3)} {MANY("seconde", $time)} sans accumulation
        [remove]{ $chance ->
                    [1] Supprime
                    *[other] supprimer
                } {NATURALFIXED($time, 3)} {MANY("seconde", $time)} de {LOC($key)}
    }

reagent-effect-guidebook-set-solution-temperature-effect =
    { $chance ->
        [1] Fixe
        *[other] fixer
    } la température de la solution exactement à {NATURALFIXED($temperature, 2)}k

reagent-effect-guidebook-adjust-solution-temperature-effect =
    { $chance ->
        [1] { $deltasign ->
                [1] Ajoute
                *[-1] Retire
            }
        *[other]
            { $deltasign ->
                [1] ajouter
                *[-1] retirer
            }
    } de la chaleur { $deltasign ->
                [1] à
                *[-1] de
           } la solution jusqu'à ce qu'elle atteigne { $deltasign ->
                [1] au maximum {NATURALFIXED($maxtemp, 2)}k
                *[-1] au minimum {NATURALFIXED($mintemp, 2)}k
            }

reagent-effect-guidebook-adjust-reagent-reagent =
    { $chance ->
        [1] { $deltasign ->
                [1] Ajoute
                *[-1] Retire
            }
        *[other]
            { $deltasign ->
                [1] ajouter
                *[-1] retirer
            }
    } {NATURALFIXED($amount, 2)}u de {$reagent} { $deltasign ->
        [1] à
        *[-1] de
    } la solution

reagent-effect-guidebook-adjust-reagent-group =
    { $chance ->
        [1] { $deltasign ->
                [1] Ajoute
                *[-1] Retire
            }
        *[other]
            { $deltasign ->
                [1] ajouter
                *[-1] retirer
            }
    } {NATURALFIXED($amount, 2)}u de réactifs du groupe {$group} { $deltasign ->
            [1] à
            *[-1] de
        } la solution

reagent-effect-guidebook-adjust-temperature =
    { $chance ->
        [1] { $deltasign ->
                [1] Ajoute
                *[-1] Retire
            }
        *[other]
            { $deltasign ->
                [1] ajouter
                *[-1] retirer
            }
    } {POWERJOULES($amount)} de chaleur { $deltasign ->
            [1] au
            *[-1] du
        } corps dans lequel il se trouve

reagent-effect-guidebook-chem-cause-disease =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } la maladie { $disease }

reagent-effect-guidebook-chem-cause-random-disease =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } les maladies { $diseases }

reagent-effect-guidebook-jittering =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } des tremblements

reagent-effect-guidebook-chem-clean-bloodstream =
    { $chance ->
        [1] Nettoie
        *[other] nettoyer
    } le flux sanguin des autres produits chimiques

reagent-effect-guidebook-cure-disease =
    { $chance ->
        [1] Soigne
        *[other] soigner
    } les maladies

reagent-effect-guidebook-cure-eye-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Inflige
                *[-1] Soigne
            }
        *[other]
            { $deltasign ->
                [1] infliger
                *[-1] soigner
            }
    } des dommages oculaires

reagent-effect-guidebook-chem-vomit =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } des vomissements

reagent-effect-guidebook-create-gas =
    { $chance ->
        [1] Crée
        *[other] créer
    } { $moles } { $moles ->
        [1] mole
        *[other] moles
    } de { $gas }

reagent-effect-guidebook-drunk =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } l'ivresse

reagent-effect-guidebook-electrocute =
    { $chance ->
        [1] Électrocute
        *[other] électrocuter
    } le métaboliseur pendant {NATURALFIXED($time, 3)} {MANY("seconde", $time)}

reagent-effect-guidebook-emote =
    { $chance ->
        [1] Force
        *[other] forcer
    } le métaboliseur à [bold][color=white]{$emote}[/color][/bold]

reagent-effect-guidebook-extinguish-reaction =
    { $chance ->
        [1] Éteint
        *[other] éteindre
    } le feu

reagent-effect-guidebook-flammable-reaction =
    { $chance ->
        [1] Augmente
        *[other] augmenter
    } l'inflammabilité

reagent-effect-guidebook-ignite =
    { $chance ->
        [1] Enflamme
        *[other] enflammer
    } le métaboliseur

reagent-effect-guidebook-make-sentient =
    { $chance ->
        [1] Rend
        *[other] rendre
    } le métaboliseur conscient

reagent-effect-guidebook-make-polymorph =
    { $chance ->
        [1] Transforme
        *[other] transformer
    } le métaboliseur en { $entityname }

reagent-effect-guidebook-modify-bleed-amount =
    { $chance ->
        [1] { $deltasign ->
                [1] Induit
                *[-1] Réduit
            }
        *[other] { $deltasign ->
                    [1] induire
                    *[-1] réduire
                 }
    } les saignements

reagent-effect-guidebook-modify-blood-level =
    { $chance ->
        [1] { $deltasign ->
                [1] Augmente
                *[-1] Diminue
            }
        *[other] { $deltasign ->
                    [1] augmenter
                    *[-1] diminuer
                 }
    } le niveau sanguin

reagent-effect-guidebook-paralyze =
    { $chance ->
        [1] Paralyse
        *[other] paralyser
    } le métaboliseur pendant au moins {NATURALFIXED($time, 3)} {MANY("seconde", $time)}

reagent-effect-guidebook-movespeed-modifier =
    { $chance ->
        [1] Modifie
        *[other] modifier
    } la vitesse de déplacement de {NATURALFIXED($walkspeed, 3)}x pendant au moins {NATURALFIXED($time, 3)} {MANY("seconde", $time)}

reagent-effect-guidebook-reset-narcolepsy =
    { $chance ->
        [1] Repousse temporairement
        *[other] repousser temporairement
    } la narcolepsie

reagent-effect-guidebook-wash-cream-pie-reaction =
    { $chance ->
        [1] Enlève
        *[other] enlever
    } la tarte à la crème du visage

reagent-effect-guidebook-cure-zombie-infection =
    { $chance ->
        [1] Soigne
        *[other] soigner
    } une infection zombie en cours

reagent-effect-guidebook-cause-zombie-infection =
    { $chance ->
        [1] Donne
        *[other] donner
    } à un individu l'infection zombie

reagent-effect-guidebook-innoculate-zombie-infection =
    { $chance ->
        [1] Soigne
        *[other] soigner
    } une infection zombie en cours et confère une immunité aux futures infections

reagent-effect-guidebook-reduce-rotting =
    { $chance ->
        [1] Régénère
        *[other] régénérer
    } {NATURALFIXED($time, 3)} {MANY("seconde", $time)} de décomposition

reagent-effect-guidebook-area-reaction =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } une réaction de fumée ou de mousse pendant {NATURALFIXED($duration, 3)} {MANY("seconde", $duration)}

reagent-effect-guidebook-add-to-solution-reaction =
    { $chance ->
        [1] Provoque
        *[other] provoquer
    } l'ajout des produits chimiques appliqués à un objet dans son conteneur de solution interne

reagent-effect-guidebook-artifact-unlock =
    { $chance ->
        [1] Aide à
        *[other] aider à
        } déverrouiller un artéfact alien.

reagent-effect-guidebook-artifact-durability-restore =
    Restaure {$restored} de durabilité dans les nœuds actifs d'artéfacts aliens.

reagent-effect-guidebook-plant-attribute =
    { $chance ->
        [1] Ajuste
        *[other] ajuster
    } {$attribute} de [color={$colorName}]{$amount}[/color]

reagent-effect-guidebook-plant-cryoxadone =
    { $chance ->
        [1] Rajeunit
        *[other] rajeunir
    } la plante, en fonction de son âge et de son temps de croissance

reagent-effect-guidebook-plant-phalanximine =
    { $chance ->
        [1] Restaure
        *[other] restaurer
    } la viabilité d'une plante rendue non viable par une mutation

reagent-effect-guidebook-plant-diethylamine =
    { $chance ->
        [1] Augmente
        *[other] augmenter
    } la durée de vie et/ou la santé de base de la plante avec 10 % de chance pour chacune

reagent-effect-guidebook-plant-robust-harvest =
    { $chance ->
        [1] Augmente
        *[other] augmenter
    } la puissance de la plante de {$increase} jusqu'à un maximum de {$limit}. Provoque la perte des graines une fois que la puissance atteint {$seedlesstreshold}. Tenter d'ajouter de la puissance au-delà de {$limit} peut entraîner une diminution du rendement avec 10 % de chance

reagent-effect-guidebook-plant-seeds-add =
    { $chance ->
        [1] Restaure les
        *[other] restaurer les
    } graines de la plante

reagent-effect-guidebook-plant-seeds-remove =
    { $chance ->
        [1] Retire les
        *[other] retirer les
    } graines de la plante

reagent-effect-guidebook-add-to-chemicals =
    { $chance ->
        [1] { $deltasign ->
                [1] Ajoute
                *[-1] Retire
            }
        *[other]
            { $deltasign ->
                [1] ajouter
                *[-1] retirer
            }
    } {NATURALFIXED($amount, 2)}u de {$reagent} { $deltasign ->
        [1] à
        *[-1] de
    } la solution
