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
comp-kitchen-spike-deny-not-dead = { CAPITALIZE(THE($victim)) } can't be butchered. { CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-BE($victim) } is not dead!

comp-kitchen-spike-begin-hook-victim = { CAPITALIZE(THE($user)) } begins dragging you onto { THE($this) }!
comp-kitchen-spike-begin-hook-self = You begin dragging yourself onto { THE($this) }!

comp-kitchen-spike-kill = { CAPITALIZE(THE($user)) } has forced { THE($victim) } onto the spike, killing them instantly!

comp-kitchen-spike-suicide-other = { CAPITALIZE(THE($victim)) } has thrown themselves on a meat spike!
comp-kitchen-spike-suicide-self = You throw yourself on a meat spike!

comp-kitchen-spike-knife-needed = You need a knife to do this.
comp-kitchen-spike-remove-meat = You remove some meat from { THE($victim) }.
comp-kitchen-spike-remove-meat-last = You remove the last piece of meat from { THE($victim) }!

comp-kitchen-spike-meat-name = { $name } ({ $victim })
