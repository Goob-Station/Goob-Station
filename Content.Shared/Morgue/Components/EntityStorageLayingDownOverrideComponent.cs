// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Morgue.Components;

/// <summary>
/// Makes an entity storage only accept entities that are laying down.
/// This is true for mobs that are crit, dead or crawling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EntityStorageLayingDownOverrideComponent : Component;
