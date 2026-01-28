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
reagent-effect-guidebook-stealth-entities = Маскирует живых существ поблизости.
reagent-effect-guidebook-change-faction = Меняет фракцию существа на { $faction }.
reagent-effect-guidebook-mutate-plants-nearby = Случайным образом мутирует ближайшие растения.
reagent-effect-guidebook-dnascramble = Перемешивает ДНК существа.
reagent-effect-guidebook-change-species = Превращает цель в { $species }.
reagent-effect-guidebook-change-species-random = Превращает цель в совершенно случайный вид.
reagent-effect-guidebook-sex-change = Изменяет половую принадлежность цели.

reagent-effect-guidebook-immunity-modifier =
    { $chance ->
        [1] Изменяет
        *[other] изменяет
    } скорость повышения иммунитета на {NATURALFIXED($gainrate, 5)}, силу на {NATURALFIXED($strength, 5)} как минимум на {NATURALFIXED($time, 3)} { $time ->
        [one] секунду
        [few] секунды
        *[other] секунд
    }

reagent-effect-guidebook-disease-progress-change =
    { $chance ->
        [1] Изменяет
        *[other] изменяет
    } прогресс заболевания с типом {$type} на {NATURALFIXED($amount, 5)}

reagent-effect-guidebook-disease-mutate = Мутирует заболевания на {NATURALFIXED($amount, 4)}