// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Nail;

[RegisterComponent, NetworkedComponent]
public sealed partial class HammerComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Weapons/smash.ogg");

    [DataField]
    public float DamageMultiplier = 1;
}
