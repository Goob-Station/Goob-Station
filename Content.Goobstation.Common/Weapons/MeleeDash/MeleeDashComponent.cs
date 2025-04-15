// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.Weapons.MeleeDash;

[RegisterComponent, NetworkedComponent]
public sealed partial class MeleeDashComponent : Component
{
    [DataField]
    public string? EmoteOnDash = "Flip"; // this sucks to have to turn into a fucking string but i dont have access to content prototypes

    [DataField]
    public SoundSpecifier? DashSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/throwhard.ogg");

    [DataField]
    public float DashForce = 15f;

    [DataField]
    public float MaxDashLength = 4f;
}

[Serializable, NetSerializable]
public sealed class MeleeDashEvent(NetEntity weapon, Vector2 direction) : EntityEventArgs
{
    public readonly NetEntity Weapon = weapon;
    public readonly Vector2 Direction = direction;
}