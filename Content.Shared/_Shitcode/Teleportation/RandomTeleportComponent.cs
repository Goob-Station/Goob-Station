// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Teleportation;

/// <summary>
///     Component to store parameters for entities that teleport randomly.
/// </summary>
[RegisterComponent, Virtual, NetworkedComponent]
public partial class RandomTeleportComponent : Component
{
    /// <summary>
    ///     Up to how far to teleport the user in tiles.
    /// </summary>
    [DataField] public MinMax Radius = new MinMax(10, 20);

    /// <summary>
    ///     How many times to try to pick the destination. Larger number means the teleport is more likely to be safe.
    /// </summary>
    [DataField] public int TeleportAttempts = 10;

    /// <summary>
    ///     Will try harder to find a safe teleport.
    /// </summary>
    [DataField] public bool ForceSafeTeleport = true;

    [DataField] public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");
    [DataField] public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
