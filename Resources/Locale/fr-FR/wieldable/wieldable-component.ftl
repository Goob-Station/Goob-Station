# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 RiceMar1244 <138547931+RiceMar1244@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Locale for wielding items; i.e. two-handing them

wieldable-verb-text-wield = Manier à deux mains
wieldable-verb-text-unwield = Ranger

wieldable-component-successful-wield = Vous maniez { THE($item) } à deux mains.
wieldable-component-failed-wield = Vous rangez { THE($item) }.
wieldable-component-successful-wield-other = { CAPITALIZE(THE($user)) } manie { THE($item) } à deux mains.
wieldable-component-failed-wield-other = { CAPITALIZE(THE($user)) } range { THE($item) }.
wieldable-component-blocked-wield = { CAPITALIZE(THE($blocker)) } vous empêche de manier { THE($item) } à deux mains.

wieldable-component-no-hands = Vous n'avez pas assez de mains !
wieldable-component-not-enough-free-hands = {$number ->
    [one] Vous avez besoin d'une main libre pour manier { THE($item) } à deux mains.
    *[other] Vous avez besoin de { $number } mains libres pour manier { THE($item) } à deux mains.
}
wieldable-component-not-in-hands = { CAPITALIZE(THE($item)) } n'est pas dans vos mains !

wieldable-component-requires = { CAPITALIZE(THE($item))} doit être maniée à deux mains !

gunwieldbonus-component-examine = Cette arme a une meilleure précision lorsqu'elle est maniée à deux mains.

gunrequireswield-component-examine = Cette arme ne peut être tirée que lorsqu'elle est maniée à deux mains.
