// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FTLDestinationComponent : Component
{
    /// <summary>
    /// Should this destination be restricted in some form from console visibility.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Is this destination visible but available to be warped to?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// Can we only FTL to beacons on this map.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BeaconsOnly;

    /// <summary>
    /// Shuttles must use a corresponding CD to travel to this location.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool RequireCoordinateDisk = false;
}