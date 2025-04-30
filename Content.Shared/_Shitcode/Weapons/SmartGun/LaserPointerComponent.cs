// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Goobstation.Weapons.SmartGun;

/// <summary>
/// Activates a laser pointer when wielding an item
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LaserPointerComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/laserpointer.ogg");

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int CollisionMask = (int) CollisionGroup.BulletImpassable;

    [DataField]
    public Color TargetedColor = Color.Green;

    [DataField]
    public Color DefaultColor = Color.Red;

    [ViewVariables]
    public TimeSpan LastNetworkEventTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan MaxDelayBetweenNetworkEvents = TimeSpan.FromSeconds(0.5);
}