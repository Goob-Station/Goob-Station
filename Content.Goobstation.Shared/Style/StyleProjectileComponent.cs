// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Style;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Style;

/// <summary>
/// Used to mark the projectile that was shot from an entity that needs to track style
/// </summary>
[RegisterComponent, Serializable]
public sealed partial class StyleProjectileComponent : Component
{
    [DataField]
    public StyleCounterComponent? Component;

    [DataField]
    public EntityUid? User;
}
