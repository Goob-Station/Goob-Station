// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared.Wall;

/// <summary>
///     This component enables an entity to ignore some obstructions for interaction checks.
/// </summary>
/// <remarks>
///     This will only exempt anchored entities that intersect the wall-mount. Additionally, this exemption will apply
///     in a limited arc, providing basic functionality for directional wall mounts.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WallMountComponent : Component
{
    /// <summary>
    ///     Range of angles for which the exemption applies. Bigger is more permissive.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("arc"), AutoNetworkedField]
    public Angle Arc = new(MathF.PI);

    /// <summary>
    ///     The direction in which the exemption arc is facing, relative to the entity's rotation. Defaults to south.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("direction"), AutoNetworkedField]
    public Angle Direction = Angle.Zero;
}
