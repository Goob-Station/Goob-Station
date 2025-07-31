# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-guidebook-deal-stamina-damage =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Наносит
               *[-1] Восстанавливает
            }
       *[other]
            { $deltasign ->
                [1] наносит
               *[-1] восстанавливает
            }
    } { $amount } { $immediate ->
        [true] немедленный
       *[false] постепенный
    } урон выносливости
