# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Loc for the pneumatic cannon.

pneumatic-cannon-component-itemslot-name = Газовый баллон

## Shown when trying to fire, but no gas

pneumatic-cannon-component-fire-no-gas = { CAPITALIZE($cannon) } щёлкает, но газ не выходит.

## Shown when changing power.

pneumatic-cannon-component-change-power =
    { $power ->
        [High] Вы устанавливаете ограничитель на максимум. Как бы вышло не слишком сильно...
        [Medium] Вы устанавливаете ограничитель посередине.
       *[Low] Вы устанавливаете ограничитель на минимум.
    }

## Shown when being stunned by having the power too high.

pneumatic-cannon-component-power-stun = { CAPITALIZE($cannon) } сбивает вас с ног!
