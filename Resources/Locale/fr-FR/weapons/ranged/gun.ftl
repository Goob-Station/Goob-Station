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


gun-selected-mode-examine = Le mode de tir sélectionné est [color={$color}]{$mode}[/color].
gun-fire-rate-examine = La cadence de tir est de [color={$color}]{$fireRate}[/color] par seconde.
gun-selector-verb = Passer à {$mode}
gun-selected-mode = {$mode} sélectionné
gun-disabled = Vous ne pouvez pas utiliser d'armes à feu !
gun-set-fire-mode = Réglé sur {$mode}
gun-magazine-whitelist-fail = Ça ne rentre pas dans l'arme !
gun-magazine-fired-empty = Plus de munitions !

# SelectiveFire
gun-SemiAuto = semi-auto
gun-Burst = rafale
gun-FullAuto = full-auto

# BallisticAmmoProvider
gun-ballistic-cycle = Éjecter
gun-ballistic-cycled = Éjecté
gun-ballistic-cycled-empty = Éjecté (vide)
gun-ballistic-transfer-invalid = {CAPITALIZE(THE($ammoEntity))} ne rentre pas dans {THE($targetEntity)} !
gun-ballistic-transfer-empty = {CAPITALIZE(THE($entity))} est vide.
gun-ballistic-transfer-target-full = {CAPITALIZE(THE($entity))} est déjà pleinement chargé.

# CartridgeAmmo
gun-cartridge-spent = Il est [color=red]usé[/color].
gun-cartridge-unspent = Il est [color=lime]neuf[/color].

# BatteryAmmoProvider
gun-battery-examine = Il a assez de charge pour [color={$color}]{$count}[/color] tir(s).

# CartridgeAmmoProvider
gun-chamber-bolt-ammo = Arme non verouillée
gun-chamber-bolt = La culasse est [color={$color}]{$bolt}[/color].
gun-chamber-bolt-closed = Culasse fermée
gun-chamber-bolt-opened = Culasse ouverte
gun-chamber-bolt-close = Fermer la culasse
gun-chamber-bolt-open = Ouvrir la culasse
gun-chamber-bolt-closed-state = ouverte
gun-chamber-bolt-open-state = fermée
gun-chamber-rack = Armer

# MagazineAmmoProvider
gun-magazine-examine = Il reste [color={$color}]{$count}[/color] tir(s).

# RevolverAmmoProvider
gun-revolver-empty = Revolver vide
gun-revolver-full = Revolver plein
gun-revolver-insert = Inséré
gun-revolver-spin = Faire tourner le revolver
gun-revolver-spun = Tourné
gun-speedloader-empty = Chargeur rapide vide

# GunSpreadModifier
examine-gun-spread-modifier-reduction = La dispersion a été réduite de [color=yellow]{$percentage}%[/color].
examine-gun-spread-modifier-increase = La dispersion a été augmentée de [color=yellow]{$percentage}%[/color].
