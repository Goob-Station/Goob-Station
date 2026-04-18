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

wieldable-verb-text-wield = Взять в руки
wieldable-verb-text-unwield = Взять в руку

wieldable-component-successful-wield = Вы берёте { $item } в две руки.
wieldable-component-failed-wield = Вы берёте { $item } в одну руку.
wieldable-component-successful-wield-other = { $user } берёт { $item } в две руки.
wieldable-component-failed-wield-other = { $user } берёт { $item } в одну руку.
wieldable-component-blocked-wield = { CAPITALIZE($blocker) } не даёт вам взять { $item } в две руки.

wieldable-component-no-hands = Вам не хватает рук!
wieldable-component-not-enough-free-hands =
    Чтобы использовать { $item } вам понадобится ещё { $number } { $number ->
        [one] свободная рука
        [few] свободные руки
       *[other] свободных рук
    }.
wieldable-component-not-in-hands = { CAPITALIZE($item) } не в ваших руках!

wieldable-component-requires = { CAPITALIZE($item) } должно быть в двух руках!

gunwieldbonus-component-examine = Это оружие обладает повышенной точностью, когда его держат в двух руках.

gunrequireswield-component-examine = Из этого оружия можно стрелять только держа его в двух руках.
