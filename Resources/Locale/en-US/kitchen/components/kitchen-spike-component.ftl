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

comp-kitchen-spike-deny-collect = { CAPITALIZE(THE($this)) } already has something on it, finish collecting its meat first!
comp-kitchen-spike-deny-butcher = { CAPITALIZE(THE($victim)) } can't be butchered on { THE($this) }.
comp-kitchen-spike-deny-changeling = { CAPITALIZE(THE($victim)) } resists being put on { THE($this) }.
comp-kitchen-spike-deny-absorbed = { CAPITALIZE(THE($victim)) } has nothing left to butcher.
comp-kitchen-spike-deny-butcher-knife = { CAPITALIZE(THE($victim)) } can't be butchered on { THE($this) }, you need to butcher it using a knife.
comp-kitchen-spike-deny-not-dead = { CAPITALIZE(THE($victim)) } can't be butchered. { CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-BE($victim) } not dead!
comp-kitchen-spike-begin-hook-self = You begin dragging yourself onto { THE($hook) }!
comp-kitchen-spike-begin-hook-self-other = { CAPITALIZE(THE($victim)) } begins dragging { REFLEXIVE($victim) } onto { THE($hook) }!

comp-kitchen-spike-begin-hook-other-self = You begin dragging { CAPITALIZE(THE($victim)) } onto { THE($hook) }!
comp-kitchen-spike-begin-hook-other = { CAPITALIZE(THE($user)) } begins dragging { CAPITALIZE(THE($victim)) } onto { THE($hook) }!

comp-kitchen-spike-hook-self = You threw yourself on { THE($hook) }!
comp-kitchen-spike-hook-self-other = { CAPITALIZE(THE($victim)) } threw { REFLEXIVE($victim) } on { THE($hook) }!

comp-kitchen-spike-hook-other-self = You threw { CAPITALIZE(THE($victim)) } on { THE($hook) }!
comp-kitchen-spike-hook-other = { CAPITALIZE(THE($user)) } threw { CAPITALIZE(THE($victim)) } on { THE($hook) }!

comp-kitchen-spike-begin-unhook-self = You begin dragging yourself off { THE($hook) }!
comp-kitchen-spike-begin-unhook-self-other = { CAPITALIZE(THE($victim)) } begins dragging { REFLEXIVE($victim) } off { THE($hook) }!

comp-kitchen-spike-begin-unhook-other-self = You begin dragging { CAPITALIZE(THE($victim)) } off { THE($hook) }!
comp-kitchen-spike-begin-unhook-other = { CAPITALIZE(THE($user)) } begins dragging { CAPITALIZE(THE($victim)) } off { THE($hook) }!

comp-kitchen-spike-unhook-self = You got yourself off { THE($hook) }!
comp-kitchen-spike-unhook-self-other = { CAPITALIZE(THE($victim)) } got { REFLEXIVE($victim) } off { THE($hook) }!

comp-kitchen-spike-unhook-other-self = You got { CAPITALIZE(THE($victim)) } off { THE($hook) }!
comp-kitchen-spike-unhook-other = { CAPITALIZE(THE($user)) } got { CAPITALIZE(THE($victim)) } off { THE($hook) }!

comp-kitchen-spike-begin-butcher-self = You begin butchering { THE($victim) }!
comp-kitchen-spike-begin-butcher = { CAPITALIZE(THE($user)) } begins to butcher { THE($victim) }!

comp-kitchen-spike-butcher-self = You butchered { THE($victim) }!
comp-kitchen-spike-butcher = { CAPITALIZE(THE($user)) } butchered { THE($victim) }!

comp-kitchen-spike-unhook-verb = Unhook

comp-kitchen-spike-hooked = [color=red]{ CAPITALIZE(THE($victim)) } is on this spike![/color]

comp-kitchen-spike-meat-name = { $name } ({ $victim })

comp-kitchen-spike-victim-examine = [color=orange]{ CAPITALIZE(SUBJECT($target)) } looks quite lean.[/color]
