# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 PixelTK <85175107+PixelTheKermit@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Errant <35878406+errant@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 MendaxxDev <153332064+MendaxxDev@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 TaralGit <76408146+TaralGit@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 and_a <and_a@DESKTOP-RJENGIR>
# SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


gun-selected-mode-examine = Current selected fire mode is [color={$color}]{$mode}[/color].
gun-fire-rate-examine = Fire rate is [color={$color}]{$fireRate}[/color] per second.
gun-selector-verb = Change to {$mode}
gun-selected-mode = Selected {$mode}
gun-disabled = You can't use guns!
gun-clumsy = The gun blows up in your face!
gun-set-fire-mode = Set to {$mode}
gun-magazine-whitelist-fail = That won't fit into the gun!
gun-magazine-fired-empty = No ammo left!

# SelectiveFire
gun-SemiAuto = semi-auto
gun-Burst = burst
gun-FullAuto = full-auto

# BallisticAmmoProvider
gun-ballistic-cycle = Cycle
gun-ballistic-cycled = Cycled
gun-ballistic-cycled-empty = Cycled (empty)
gun-ballistic-transfer-invalid = {CAPITALIZE(THE($ammoEntity))} won't fit inside {THE($targetEntity)}!
gun-ballistic-transfer-empty = {CAPITALIZE(THE($entity))} is empty.
gun-ballistic-transfer-target-full = {CAPITALIZE(THE($entity))} is already fully loaded.

# CartridgeAmmo
gun-cartridge-spent = It is [color=red]spent[/color].
gun-cartridge-unspent = It is [color=lime]not spent[/color].

# BatteryAmmoProvider
gun-battery-examine = It has enough charge for [color={$color}]{$count}[/color] shots.

# CartridgeAmmoProvider
gun-chamber-bolt-ammo = Gun not bolted
gun-chamber-bolt = The bolt is [color={$color}]{$bolt}[/color].
gun-chamber-bolt-closed = Closed bolt
gun-chamber-bolt-opened = Opened bolt
gun-chamber-bolt-close = Close bolt
gun-chamber-bolt-open = Open bolt
gun-chamber-bolt-closed-state = open
gun-chamber-bolt-open-state = closed
gun-chamber-rack = Rack

# MagazineAmmoProvider
gun-magazine-examine = It has [color={$color}]{$count}[/color] shots remaining.

# RevolverAmmoProvider
gun-revolver-empty = Empty revolver
gun-revolver-full = Revolver full
gun-revolver-insert = Inserted
gun-revolver-spin = Spin revolver
gun-revolver-spun = Spun
gun-speedloader-empty = Speedloader empty
