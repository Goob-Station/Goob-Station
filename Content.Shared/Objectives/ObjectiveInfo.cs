// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Objectives.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Objectives;

/// <summary>
/// Info about objectives visible in the character menu and on round end.
/// Description and icon are displayed only in the character menu.
/// Progress is a percentage from 0.0 to 1.0.
/// </summary>
/// <remarks>
/// All of these fields must eventually be set by condition event handlers.
/// Everything but progress can be set to static data in yaml on the entity and <see cref="ObjectiveComponent"/>.
/// If anything is null it will be logged and return null.
/// </remarks>
[Serializable, NetSerializable]
public record struct ObjectiveInfo(string Title, string Description, SpriteSpecifier Icon, float Progress);