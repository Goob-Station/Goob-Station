// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Skinnable;

[RegisterComponent]
public sealed partial class SkinnableComponent : Component
{
    [DataField]
    public bool Skinned;

    [DataField]
    public TimeSpan SkinningDoAfterDuation = TimeSpan.FromSeconds(5);

    [DataField]
    public DamageSpecifier DamageOnSkinned = new() { DamageDict = new Dictionary<string, FixedPoint2> { { "Slash", 50 } } };

    [DataField]
    public SoundSpecifier SkinSound = new SoundPathSpecifier("/Audio/_Shitmed/Medical/Surgery/scalpel1.ogg");
}
