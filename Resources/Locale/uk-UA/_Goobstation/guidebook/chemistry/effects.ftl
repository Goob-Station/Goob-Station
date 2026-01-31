# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-guidebook-deal-stamina-damage = { $chance ->
        [1] { $deltasign ->
                [1] Завдає
                *[-1] Лікує
            }
        *[other]
            { $deltasign ->
                [1] завдати
                *[-1] зцілити
            }
    } { $amount } { $immediate ->
                    [true] миттєвої
                    *[false] поступової
                  } шкоди витривалості

reagent-effect-guidebook-immunity-modifier = { $chance ->
        [1] Змінює
        *[other] змінюють
    } швидкість приросту імунітету на {NATURALFIXED($gainrate, 5)}, силу на {NATURALFIXED($strength, 5)} щонайменше на {NATURALFIXED($time, 3)} {MANY("second", $time)}
reagent-effect-guidebook-disease-progress-change = { $chance ->
        [1] Змінює
        *[other] змінюють
    } прогрес захворювань типу {$type} на {NATURALFIXED($amount, 5)}
reagent-effect-guidebook-disease-mutate = Мутує захворювання на {NATURALFIXED($amount, 4)}
reagent-effect-guidebook-stealth-entities = Маскує живих істот поблизу.
reagent-effect-guidebook-change-faction = Змінює фракцію істоти на {$faction}.
reagent-effect-guidebook-mutate-plants-nearby = Випадково мутує рослини поблизу.
reagent-effect-guidebook-dnascramble = Заплутує ДНК людини.
reagent-effect-guidebook-change-species = Перетворює ціль на {$species}.
reagent-effect-guidebook-sex-change = Змінює гендер людини
reagent-effect-guidebook-change-species-random = Перетворює ціль на абсолютно випадковий вид.