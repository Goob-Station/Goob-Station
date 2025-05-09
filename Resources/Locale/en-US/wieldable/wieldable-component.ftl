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

wieldable-verb-text-wield = Wield
wieldable-verb-text-unwield = Unwield

wieldable-component-successful-wield = You wield { THE($item) }.
wieldable-component-failed-wield = You unwield { THE($item) }.
wieldable-component-successful-wield-other = { CAPITALIZE(THE($user)) } wields { THE($item) }.
wieldable-component-failed-wield-other = { CAPITALIZE(THE($user)) } unwields { THE($item) }.

wieldable-component-no-hands = You don't have enough hands!
wieldable-component-not-enough-free-hands = {$number ->
    [one] You need a free hand to wield { THE($item) }.
    *[other] You need { $number } free hands to wield { THE($item) }.
}
wieldable-component-not-in-hands = { CAPITALIZE(THE($item)) } isn't in your hands!

wieldable-component-requires = { CAPITALIZE(THE($item))} must be wielded!

gunwieldbonus-component-examine = This weapon has improved accuracy when wielded.

gunrequireswield-component-examine = This weapon can only be fired when wielded.
