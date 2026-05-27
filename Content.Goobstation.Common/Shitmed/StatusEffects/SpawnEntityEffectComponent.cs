// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

/*
    2026-05-26
    This is in Common because Content.Shared/_Shitmed/StatusEffects/SpawnGravityWellComponent.cs
    depends on this and is also in common.

    If/when that file is moved into Content.Goobstation.Shared, this can be moved out of common and into shared as well.
*/

namespace Content.Goobstation.Common.Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Spawns a given entity prototype.
/// </summary>
public abstract partial class SpawnEntityEffectComponent : Component
{
    public virtual string EntityPrototype { get; set; }

    public virtual bool IsFriendly { get; set; }

    public virtual bool AttachToParent { get; set; }
}