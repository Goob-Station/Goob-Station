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

entity-condition-guidebook-total-damage =
    { $max ->
        [2147483648] it has at least {NATURALFIXED($min, 2)} total damage
        *[other] { $min ->
                    [0] it has at most {NATURALFIXED($max, 2)} total damage
                    *[other] it has between {NATURALFIXED($min, 2)} and {NATURALFIXED($max, 2)} total damage
                 }
    }

entity-condition-guidebook-type-damage =
    { $max ->
        [2147483648] it has at least {NATURALFIXED($min, 2)} of {$type} damage
        *[other] { $min ->
                    [0] it has at most {NATURALFIXED($max, 2)} of {$type} damage
                    *[other] it has between {NATURALFIXED($min, 2)} and {NATURALFIXED($max, 2)} of {$type} damage
                 }
    }

entity-condition-guidebook-group-damage =
    { $max ->
        [2147483648] it has at least {NATURALFIXED($min, 2)} of {$type} damage.
        *[other] { $min ->
                    [0] it has at most {NATURALFIXED($max, 2)} of {$type} damage.
                    *[other] it has between {NATURALFIXED($min, 2)} and {NATURALFIXED($max, 2)} of {$type} damage
                 }
    }

entity-condition-guidebook-total-hunger =
    { $max ->
        [2147483648] the target has at least {NATURALFIXED($min, 2)} total hunger
        *[other] { $min ->
                    [0] the target has at most {NATURALFIXED($max, 2)} total hunger
                    *[other] the target has between {NATURALFIXED($min, 2)} and {NATURALFIXED($max, 2)} total hunger
                 }
    }

entity-condition-guidebook-reagent-threshold =
    { $max ->
        [2147483648] there's at least {NATURALFIXED($min, 2)}u of {$reagent}
        *[other] { $min ->
                    [0] there's at most {NATURALFIXED($max, 2)}u of {$reagent}
                    *[other] there's between {NATURALFIXED($min, 2)}u and {NATURALFIXED($max, 2)}u of {$reagent}
                 }
    }

entity-condition-guidebook-mob-state-condition =
    the mob is { $state }

entity-condition-guidebook-job-condition =
    the target's job is { $job }

entity-condition-guidebook-solution-temperature =
    the solution's temperature is { $max ->
            [2147483648] at least {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] at most {NATURALFIXED($max, 2)}k
                        *[other] between {NATURALFIXED($min, 2)}k and {NATURALFIXED($max, 2)}k
                     }
    }

entity-condition-guidebook-body-temperature =
    the body's temperature is { $max ->
            [2147483648] at least {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] at most {NATURALFIXED($max, 2)}k
                        *[other] between {NATURALFIXED($min, 2)}k and {NATURALFIXED($max, 2)}k
                     }
    }

entity-condition-guidebook-organ-type =
    the metabolizing organ { $shouldhave ->
                                [true] is
                                *[false] is not
                           } {INDEFINITE($name)} {$name} organ

entity-condition-guidebook-has-tag =
    the target { $invert ->
                 [true] does not have
                 *[false] has
                } the tag {$tag}

entity-condition-guidebook-this-reagent = this reagent

# Goobstation?
entity-condition-guidebook-blood-reagent-threshold =
    { $max ->
        [2147483648] there's at least {NATURALFIXED($min, 2)}u of {$reagent}
        *[other] { $min ->
                    [0] there's at most {NATURALFIXED($max, 2)}u of {$reagent}
                    *[other] there's between {NATURALFIXED($min, 2)}u and {NATURALFIXED($max, 2)}u of {$reagent}
                 }
    }
# Goobstation?

entity-condition-guidebook-breathing =
    the metabolizer is { $isBreathing ->
                [true] breathing normally
                *[false] suffocating
               }

entity-condition-guidebook-internals =
    the metabolizer is { $usingInternals ->
                [true] using internals
                *[false] breathing atmospheric air
               }
