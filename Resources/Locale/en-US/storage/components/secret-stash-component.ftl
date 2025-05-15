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

comp-secret-stash-action-hide-success = You hide { THE($item) } in the {$stashname}.
comp-secret-stash-action-hide-container-not-empty = There's already something in here!?
comp-secret-stash-action-hide-item-too-big = { CAPITALIZE(THE($item)) } is too big to fit in the {$stashname}.
comp-secret-stash-action-get-item-found-something = There was something inside the {$stashname}!
comp-secret-stash-on-examine-found-hidden-item = There is something hidden inside the {$stashname}!
comp-secret-stash-on-destroyed-popup = Something falls out of the the {$stashname}!

### Verbs
comp-secret-stash-verb-insert-into-stash = Stash item
comp-secret-stash-verb-insert-message-item-already-inside = There is already an item inside the {$stashname}.
comp-secret-stash-verb-insert-message-no-item = Hide { THE($item) } in the {$stashname}.
comp-secret-stash-verb-take-out-item = Grab item
comp-secret-stash-verb-take-out-message-something = Take the contents of the {$stashname} out.
comp-secret-stash-verb-take-out-message-nothing = There is nothing inside the {$stashname}.

comp-secret-stash-verb-close = Close
comp-secret-stash-verb-cant-close = You can't close the {$stashname} with that.
comp-secret-stash-verb-open = Open

### Stash names
secret-stash-plant = plant
secret-stash-toilet = toilet cistern
secret-stash-plushie = plushie
