// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Tuning for the Gyroscope ability: crushes living beings the core lands on.
/// </summary>
[RegisterComponent]
public sealed partial class MalfGyroscopeComponent : Component
{
    /// <summary>Damage dealt to bodiless mobs the core lands on. Bodied creatures are gibbed instead.</summary>
    [DataField] public DamageSpecifier CrushDamage = new()
    {
        DamageDict = new() { { "Blunt", 200 } },
    };

    /// <summary>Sound played when the core crushes someone.</summary>
    [DataField] public SoundSpecifier CrushSound = new SoundCollectionSpecifier("gib");
}
