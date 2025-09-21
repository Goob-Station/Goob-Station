// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 root <root@DESKTOP-HJPF29C>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Radiation.Systems;

namespace Content.Server.Radiation.Components;

/// <summary>
///     Prevents entities from emitting or receiving radiation when placed inside this container.
/// </summary>
[RegisterComponent]
[Access(typeof(RadiationSystem))]
public sealed partial class RadiationBlockingContainerComponent : Component
{
    // Goobstation - Radiation Rework: Default value changed to 0f
    /// <summary>
    ///     Flat reduction in radiation when an item is in the container.
    /// </summary>
    [DataField("resistance")]
    public float RadResistance = 0f;

    // Goobstation - Radiation Rework
    /// <summary>
    ///     TODO: Clarify this.
    ///     Radiation decay for the Goobstation radiation overhaul after applying the flat reduction.
    /// </summary>
    [DataField("decay")]
    public float RadDecay = 1f;
}
