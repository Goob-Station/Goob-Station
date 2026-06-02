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

comp-secret-stash-action-hide-success = Vous cachez { THE($item) } dans {$stashname}.
comp-secret-stash-action-hide-container-not-empty = Il y a déjà quelque chose ici !?
comp-secret-stash-action-hide-item-too-big = { CAPITALIZE(THE($item)) } est trop grand pour entrer dans {$stashname}.
comp-secret-stash-action-get-item-found-something = Il y avait quelque chose à l'intérieur de {$stashname} !
comp-secret-stash-on-examine-found-hidden-item = Il y a quelque chose de caché à l'intérieur de {$stashname} !
comp-secret-stash-on-destroyed-popup = Quelque chose tombe de {$stashname} !

### Verbs
comp-secret-stash-verb-insert-into-stash = Cacher l'objet
comp-secret-stash-verb-insert-message-item-already-inside = Il y a déjà un objet à l'intérieur de {$stashname}.
comp-secret-stash-verb-insert-message-no-item = Cacher { THE($item) } dans {$stashname}.
comp-secret-stash-verb-take-out-item = Prendre l'objet
comp-secret-stash-verb-take-out-message-something = Sortir le contenu de {$stashname}.
comp-secret-stash-verb-take-out-message-nothing = Il n'y a rien à l'intérieur de {$stashname}.

comp-secret-stash-verb-close = Fermer
comp-secret-stash-verb-cant-close = Vous ne pouvez pas fermer {$stashname} avec ça.
comp-secret-stash-verb-open = Ouvrir

### Stash names
secret-stash-plant = plante
secret-stash-toilet = réservoir de toilettes
secret-stash-plushie = peluche
secret-stash-cake = gâteau
