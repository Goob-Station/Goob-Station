# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Loc for the pneumatic cannon.

pneumatic-cannon-component-itemslot-name = Gas Tank

## Shown when trying to fire, but no gas

pneumatic-cannon-component-fire-no-gas = { CAPITALIZE(THE($cannon)) } clicks, but no gas comes out.

## Shown when changing power.

pneumatic-cannon-component-change-power = { $power ->
    [High] You set the limiter to maximum power. It feels a little too powerful...
    [Medium] You set the limiter to medium power.
    *[Low] You set the limiter to low power.
}

## Shown when being stunned by having the power too high.

pneumatic-cannon-component-power-stun = The pure force of { THE($cannon) } knocks you over!

