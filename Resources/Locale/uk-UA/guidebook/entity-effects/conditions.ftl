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

entity-condition-guidebook-total-damage = { $max ->
        [2147483648] має щонайменше {NATURALFIXED($min, 2)} загальної шкоди
        *[other] { $min ->
                    [0] має щонайбільше {NATURALFIXED($max, 2)} загальної шкоди
                    *[other] має від {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} загальної шкоди
                 }
    }
entity-condition-guidebook-type-damage = { $max ->
        [2147483648] має щонайменше {NATURALFIXED($min, 2)} шкоди типу {$type}
        *[other] { $min ->
                    [0] має щонайбільше {NATURALFIXED($max, 2)} шкоди типу {$type}
                    *[other] має від {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} шкоди типу {$type}
                 }
    }
entity-condition-guidebook-group-damage = { $max ->
        [2147483648] має щонайменше {NATURALFIXED($min, 2)} шкоди групи {$type}.
        *[other] { $min ->
                    [0] має щонайбільше {NATURALFIXED($max, 2)} шкоди групи {$type}.
                    *[other] має від {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} шкоди групи {$type}
                 }
    }
entity-condition-guidebook-total-hunger = { $max ->
        [2147483648] ціль має щонайменше {NATURALFIXED($min, 2)} загального голоду
        *[other] { $min ->
                    [0] ціль має щонайбільше {NATURALFIXED($max, 2)} загального голоду
                    *[other] ціль має від {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} загального голоду
                 }
    }
entity-condition-guidebook-reagent-threshold = { $max ->
        [2147483648] є щонайменше {NATURALFIXED($min, 2)}u {$reagent}
        *[other] { $min ->
                    [0] є щонайбільше {NATURALFIXED($max, 2)}u {$reagent}
                    *[other] є від {NATURALFIXED($min, 2)}u до {NATURALFIXED($max, 2)}u {$reagent}
                 }
    }
entity-condition-guidebook-mob-state-condition = моб перебуває у стані { $state }
entity-condition-guidebook-job-condition = посада цілі - { $job }
entity-condition-guidebook-solution-temperature = температура розчину { $max ->
            [2147483648] щонайменше {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] щонайбільше {NATURALFIXED($max, 2)}k
                        *[other] від {NATURALFIXED($min, 2)}k до {NATURALFIXED($max, 2)}k
                     }
    }
entity-condition-guidebook-body-temperature = температура тіла { $max ->
            [2147483648] щонайменше {NATURALFIXED($min, 2)}k
            *[other] { $min ->
                        [0] щонайбільше {NATURALFIXED($max, 2)}k
                        *[other] від {NATURALFIXED($min, 2)}k до {NATURALFIXED($max, 2)}k
                     }
    }
entity-condition-guidebook-organ-type = орган, що метаболізує, { $shouldhave ->
                                [true] є
                                *[false] не є
                           } органом {INDEFINITE($name)} {$name}
entity-condition-guidebook-has-tag = ціль { $invert ->
                 [true] не має
                 *[false] має
                } тег {$tag}
entity-condition-guidebook-this-reagent = цей реагент
entity-condition-guidebook-blood-reagent-threshold = { $max ->
        [2147483648] є щонайменше {NATURALFIXED($min, 2)}u {$reagent}
        *[other] { $min ->
                    [0] є щонайбільше {NATURALFIXED($max, 2)}u {$reagent}
                    *[other] є від {NATURALFIXED($min, 2)}u до {NATURALFIXED($max, 2)}u {$reagent}
                 }
    }
entity-condition-guidebook-breathing = метаболізатор { $isBreathing ->
                [true] дихає нормально
                *[false] задихається
               }
entity-condition-guidebook-internals = метаболізатор { $usingInternals ->
                [true] використовує дихальний балон
                *[false] дихає атмосферним повітрям
               }