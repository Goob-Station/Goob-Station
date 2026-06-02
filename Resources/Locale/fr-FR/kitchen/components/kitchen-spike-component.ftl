# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 FoLoKe <36813380+FoLoKe@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

comp-kitchen-spike-deny-collect = { CAPITALIZE(THE($this)) } a déjà quelque chose dessus, finissez de récupérer sa viande d'abord !
comp-kitchen-spike-deny-butcher = { CAPITALIZE(THE($victim)) } ne peut pas être dépecé sur { THE($this) }.
comp-kitchen-spike-deny-changeling = { CAPITALIZE(THE($victim)) } résiste à être mis sur { THE($this) }.
comp-kitchen-spike-deny-absorbed = { CAPITALIZE(THE($victim)) } n'a plus rien à dépecer.
comp-kitchen-spike-deny-butcher-knife = { CAPITALIZE(THE($victim)) } ne peut pas être dépecé sur { THE($this) }, vous devez le dépecer avec un couteau.
comp-kitchen-spike-deny-not-dead = { CAPITALIZE(THE($victim)) } ne peut pas être dépecé. { CAPITALIZE(SUBJECT($victim)) } n'est pas mort !

comp-kitchen-spike-begin-hook-victim = { CAPITALIZE(THE($user)) } commence à vous traîner sur { THE($this) } !
comp-kitchen-spike-begin-hook-self = Vous commencez à vous traîner sur { THE($this) } !

comp-kitchen-spike-kill = { CAPITALIZE(THE($user)) } a forcé { THE($victim) } sur { THE($this) }, le tuant instantanément !

comp-kitchen-spike-suicide-other = { CAPITALIZE(THE($victim)) } s'est jeté sur { THE($this) } !
comp-kitchen-spike-suicide-self = Vous vous jetez sur { THE($this) } !

comp-kitchen-spike-knife-needed = Vous avez besoin d'un couteau pour faire cela.
comp-kitchen-spike-remove-meat = Vous retirez de la viande de { THE($victim) }.
comp-kitchen-spike-remove-meat-last = Vous retirez le dernier morceau de viande de { THE($victim) } !

comp-kitchen-spike-meat-name = { $name } ({ $victim })
