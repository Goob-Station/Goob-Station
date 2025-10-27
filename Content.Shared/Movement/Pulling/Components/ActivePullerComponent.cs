// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Movement.Pulling.Components;

/// <summary>
/// Component that indicates that an entity is currently pulling some other entity.
/// </summary>
[RegisterComponent]
public sealed partial class ActivePullerComponent : Component;