# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-guidebook-deal-stamina-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Deals
                *[-1] Heals
            }
        *[other]
            { $deltasign ->
                [1] deal
                *[-1] heal
            }
    } { $amount } { $immediate ->
                    [true] immediate
                    *[false] overtime
                  } stamina damage

reagent-effect-guidebook-stealth-entities = Camouflages living mobs nearby.

reagent-effect-guidebook-change-faction = Changes the mob's faction to {$faction}.

reagent-effect-guidebook-mutate-plants-nearby = Randomly mutates nearby plants.

reagent-effect-guidebook-dnascramble = Scrambles the person's DNA.

reagent-effect-guidebook-change-species = Turns the target into a {$species}

reagent-effect-guidebook-sex-change = Swaps the person's gender

reagent-effect-guidebook-immunity-modifier =
    { $chance ->
        [1] Modifies
        *[other] modify
    } immunity gain rate by {NATURALFIXED($gainrate, 5)}, strength by {NATURALFIXED($strength, 5)} for at least {NATURALFIXED($time, 3)} {MANY("second", $time)}

reagent-effect-guidebook-disease-progress-change =
    { $chance ->
        [1] Modifies
        *[other] modify
    } progress of {$type} diseases by {NATURALFIXED($amount, 5)}

reagent-effect-guidebook-disease-mutate = Mutates diseases by {NATURALFIXED($amount, 4)}
