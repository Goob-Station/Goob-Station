# SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
# SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


### UI

# Verb name for climbing
comp-climbable-verb-climb = Vault

### Interaction Messages

# Shown to you when your character climbs on $climbable
comp-climbable-user-climbs = You jump onto { THE($climbable) }!

# Shown to others when $user climbs on $climbable
comp-climbable-user-climbs-other  = { CAPITALIZE(THE($user)) } jumps onto { THE($climbable) }!

# Shown to you when your character forces someone to climb on $climbable
comp-climbable-user-climbs-force = You force { THE($moved-user) } onto { THE($climbable) }!

# Shown to others when someone forces other $moved-user to climb on $climbable
comp-climbable-user-climbs-force-other = { CAPITALIZE(THE($user)) } forces { THE($moved-user) } onto { THE($climbable) }!

# Shown to you when your character is far away from climbable
comp-climbable-cant-reach = You can't reach there!

# Shown to you when your character can't interact with climbable for some reason
comp-climbable-cant-interact = You can't do that!

# Shown to you when your character isn't able to climb by their own actions
comp-climbable-cant-climb = You are incapable of climbing!

# Shown to you when your character tries to force someone else who can't climb onto a climbable
comp-climbable-target-cant-climb = { CAPITALIZE(THE($moved-user)) } can't go there!
