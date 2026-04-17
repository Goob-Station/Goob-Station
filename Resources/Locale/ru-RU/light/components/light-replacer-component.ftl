# SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


### Interaction Messages

# Shown when player tries to replace light, but there is no lights left
comp-light-replacer-missing-light = В { $light-replacer } не осталось лампочек.

# Shown when player inserts light bulb inside light replacer
comp-light-replacer-insert-light = Вы вставили { $bulb } в { $light-replacer }.

# Shown when player tries to insert in light replacer brolen light bulb
comp-light-replacer-insert-broken-light = Вы не можете вставлять разбитые лампочки!

# Shown when player refill light from light box
comp-light-replacer-refill-from-storage = Вы пополнили { $light-replacer }.

### Examine

comp-light-replacer-no-lights = Здесь пусто.
comp-light-replacer-has-lights = Здесь находится следующее:
comp-light-replacer-light-listing = [color=yellow]{ $amount }[/color] ед. [color=gray]{ $name }[/color]
