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

comp-kitchen-spike-deny-collect = { CAPITALIZE($this) } уже чем-то занят, сначала закончите срезать мясо!
comp-kitchen-spike-deny-butcher = { CAPITALIZE($victim) } не может быть разделан на { $this }.
comp-kitchen-spike-deny-changeling = { CAPITALIZE($victim) } resists being put on { $this }.
comp-kitchen-spike-deny-absorbed = { CAPITALIZE($victim) } has nothing left to butcher.
comp-kitchen-spike-deny-butcher-knife = { CAPITALIZE($victim) } не может быть разделан на { $this }, используйте нож для разделки.
comp-kitchen-spike-deny-not-dead =
    { CAPITALIZE($victim) } не может быть разделан. { CAPITALIZE(SUBJECT($victim)) } { GENDER($victim) ->
        [male] ещё жив
        [female] ещё жива
        [epicene] ещё живы
       *[neuter] ещё живо
    }!

comp-kitchen-spike-begin-hook-victim = { CAPITALIZE($user) } начинает насаживать вас на { $this }!
comp-kitchen-spike-begin-hook-self = Вы начинаете насаживать себя на { $this }!

comp-kitchen-spike-kill = { CAPITALIZE($user) } насаживает { $victim } на мясной крюк, тем самым убивая { SUBJECT($victim) }!

comp-kitchen-spike-suicide-other = { CAPITALIZE($victim) } бросается на мясной крюк!
comp-kitchen-spike-suicide-self = Вы бросаетесь на мясной крюк!

comp-kitchen-spike-knife-needed = Вам нужен нож для этого.
comp-kitchen-spike-remove-meat = Вы срезаете немного мяса с { $victim }.
comp-kitchen-spike-remove-meat-last = Вы срезаете последний кусок мяса с { $victim }!

comp-kitchen-spike-meat-name = мясо { $victim }
