# SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Secret stash component. Stuff like potted plants, comfy chair cushions, etc...

comp-secret-stash-action-hide-success = Вы прячете { $item } в { $stashname }.
comp-secret-stash-action-hide-container-not-empty = Тут уже что-то есть!?
comp-secret-stash-action-hide-item-too-big = { CAPITALIZE($item) } слишком большой, чтобы поместиться в { $stashname }.
comp-secret-stash-action-get-item-found-something = Внутри { $stashname } что-то было!
comp-secret-stash-on-examine-found-hidden-item = Внутри { $stashname } что-то спрятано!
comp-secret-stash-on-destroyed-popup = Что-то выпадает из { $stashname }!

### Verbs
comp-secret-stash-verb-insert-into-stash = Спрятать предмет
comp-secret-stash-verb-insert-message-item-already-inside = Внутри { $stashname } уже есть предмет.
comp-secret-stash-verb-insert-message-no-item = Спрятать { $item } в { $stashname }.
comp-secret-stash-verb-take-out-item = Взять предмет
comp-secret-stash-verb-take-out-message-something = Достать содержимое { $stashname }.
comp-secret-stash-verb-take-out-message-nothing = Внутри { $stashname } пусто.

comp-secret-stash-verb-close = Закрыть
comp-secret-stash-verb-cant-close = Вы не можете закрыть { $stashname } с помощью этого.
comp-secret-stash-verb-open = Открыть

### Stash names
secret-stash-plant = растение
secret-stash-toilet = туалетный бачок
secret-stash-plushie = плюшевая игрушка
secret-stash-cake = торт
