# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 KrasnoshchekovPavel <119816022+KrasnoshchekovPavel@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
# SPDX-FileCopyrightText: 2024 icekot8 <93311212+icekot8@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 potato1234_x <79580518+potato1234x@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-condition-guidebook-total-damage =
    { $max ->
        [2147483648] il a au moins {NATURALFIXED($min, 2)} de dégâts totaux
        *[other] { $min ->
                    [0] il a au plus {NATURALFIXED($max, 2)} de dégâts totaux
                    *[other] il a entre {NATURALFIXED($min, 2)} et {NATURALFIXED($max, 2)} de dégâts totaux
                 }
    }

reagent-effect-condition-guidebook-total-hunger =
    { $max ->
        [2147483648] la cible a au moins {NATURALFIXED($min, 2)} de faim totale
        *[other] { $min ->
                    [0] la cible a au plus {NATURALFIXED($max, 2)} de faim totale
                    *[other] la cible a entre {NATURALFIXED($min, 2)} et {NATURALFIXED($max, 2)} de faim totale
                 }
    }

reagent-effect-condition-guidebook-reagent-threshold =
    { $max ->
        [2147483648] il y a au moins {NATURALFIXED($min, 2)}u de {$reagent}
        *[other] { $min ->
                    [0] il y a au plus {NATURALFIXED($max, 2)}u de {$reagent}
                    *[other] il y a entre {NATURALFIXED($min, 2)}u et {NATURALFIXED($max, 2)}u de {$reagent}
                 }
    }

reagent-effect-condition-guidebook-mob-state-condition =
    la créature est { $state }

reagent-effect-condition-guidebook-job-condition =
    le poste de la cible est { $job }

reagent-effect-condition-guidebook-solution-temperature =
    la température de la solution est { $max ->
            [2147483648] d'au moins {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] d'au plus {NATURALFIXED($max, 2)}k
                        *[other] entre {NATURALFIXED($min, 2)}k et {NATURALFIXED($max, 2)}k
                     }
    }

reagent-effect-condition-guidebook-body-temperature =
    la température corporelle est { $max ->
            [2147483648] d'au moins {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] d'au plus {NATURALFIXED($max, 2)}k
                        *[other] entre {NATURALFIXED($min, 2)}k et {NATURALFIXED($max, 2)}k
                     }
    }

reagent-effect-condition-guidebook-organ-type =
    l'organe métabolisant { $shouldhave ->
                                [true] est
                                *[false] n'est pas
                           } un organe {$name}

reagent-effect-condition-guidebook-has-tag =
    la cible { $invert ->
                 [true] ne possède pas
                 *[false] possède
                } le tag {$tag}

reagent-effect-condition-guidebook-blood-reagent-threshold =
    { $max ->
        [2147483648] il y a au moins {NATURALFIXED($min, 2)}u de {$reagent}
        *[other] { $min ->
                    [0] il y a au plus {NATURALFIXED($max, 2)}u de {$reagent}
                    *[other] il y a entre {NATURALFIXED($min, 2)}u et {NATURALFIXED($max, 2)}u de {$reagent}
                 }
    }

reagent-effect-condition-guidebook-this-reagent = ce réactif

reagent-effect-condition-guidebook-breathing =
    le métaboliseur { $isBreathing ->
                [true] respire normalement
                *[false] est en train de suffoquer
               }

reagent-effect-condition-guidebook-internals =
    le métaboliseur { $usingInternals ->
                [true] utilise des réserves internes
                *[false] respire l'air ambiant
               }
               