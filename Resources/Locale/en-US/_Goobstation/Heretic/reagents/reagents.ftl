# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-name-eldritch = eldritch essence
reagent-desc-eldritch = A strange liquid that defies the laws of physics. It re-energizes and heals those who can see beyond this fragile reality, but is incredibly harmful to the closed-minded.

reagent-name-crucible-soul = crucible soul
reagent-desc-crucible-soul = A bright orange, translucent liquid. Allows you to walk through walls. After expiring, you are teleported to your original location.

reagent-name-clarity = dusk and dawn
reagent-desc-clarity = A dull yellow liquid. It seems to fade in and out with regularity. Allows you to see through walls and objects.

reagent-name-marshal = wounded soldier
reagent-desc-marshal = A colorless, dark liquid. Increases your physical strength, making your attacks more furious the more damaged you are, prevents losing consciousness and makes your melee attacks heal your health and stamina but damages you overtime.

reagent-comp-condition-heretic-or-ghoul = heretic or ghoul

reagent-effect-guidebook-has-status-effect =
    { $invert ->
        [true] has no
        *[false] has
    } {$effect} status effect.

reagent-physical-desc-eldritch = eldritch
reagent-physical-desc-crucible-soul = otherworldly
reagent-physical-desc-clarity = clear
reagent-physical-desc-marshal = agonizing

flavor-complex-eldritch = Ag'hsj'saje'sh
flavor-complex-crucible-soul = like something between the plains
flavor-complex-clarity = like eyes
flavor-complex-marshal = painful

crucible-soul-effect-examine-message =
    {"["}color=#fb793a]{ CAPITALIZE(SUBJECT($ent)) } { GENDER($ent) ->
        [epicene] do
       *[other] does
    } not seem to be all there.[/color]

wounded-solider-effect-examine-message = [color=#5e718e]{ CAPITALIZE(SUBJECT($ent)) } { CONJUGATE-BE($ent) } in a state on undying frenzy.[/color]
