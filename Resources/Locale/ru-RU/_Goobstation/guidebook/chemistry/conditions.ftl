# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-condition-guidebook-stamina-damage-threshold =
    { $max ->
        [2147483648] у цели как минимум { NATURALFIXED($min, 2) } урона выносливости
       *[other]
            { $min ->
                [0] у цели максимум { NATURALFIXED($max, 2) } урона выносливости
               *[other] у цели от { NATURALFIXED($min, 2) } до { NATURALFIXED($max, 2) } урона выносливости
            }
    }
reagent-effect-condition-guidebook-unique-bloodstream-chem-threshold =
    { $max ->
        [2147483648]
            { $min ->
                [1] в организме как минимум { $min } реагент
               *[other] в организме как минимум { $min } реагентов
            }
        [1]
            { $min ->
                [0] в организме максимум { $max } реагент
               *[other] в организме от { $min } до { $max } реагентов
            }
       *[other]
            { $min ->
                [-1] в организме максимум { $max } реагентов
               *[other] в организме от { $min } до { $max } реагентов
            }
    }
reagent-effect-condition-guidebook-typed-damage-threshold =
    { $inverse ->
        [true] у цели максимум
       *[false] у цели как минимум
    } { $changes } урона
