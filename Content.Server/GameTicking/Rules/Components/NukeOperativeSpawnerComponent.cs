// SPDX-FileCopyrightText: 2022 TekuNut <13456422+TekuNut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rainfall <rainfey0+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// This is used for tagging a spawn point as a nuke operative spawn point
/// and providing loadout + name for the operative on spawn.
/// TODO: Remove once systems can request spawns from the ghost role system directly.
/// </summary>
[RegisterComponent, EntityCategory("Spawner")]
public sealed partial class NukeOperativeSpawnerComponent : Component;