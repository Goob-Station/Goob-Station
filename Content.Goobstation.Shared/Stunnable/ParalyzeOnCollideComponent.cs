// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Stunnable;

[RegisterComponent, NetworkedComponent]
[Access(typeof(ParalyzeOnCollideSystem))]
public sealed partial class ParalyzeOnCollideComponent : Component
{
    /// <summary>
    /// Whether or not to remove this component after colliding once
    /// </summary>
    [DataField("removeAfterCollide")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool RemoveAfterCollide = true;

    /// <summary>
    /// Whether or not to remove this component after being thrown and landing
    /// </summary>
    [DataField("removeOnLand")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool RemoveOnLand = true;

    /// <summary>
    /// Entities we can collide with without paralyzing
    /// </summary>
    [DataField("collidableEntities")]
    [Access(typeof(ParalyzeOnCollideSystem), Other = AccessPermissions.ReadWrite)]
    public EntityWhitelist? CollidableEntities;

    [DataField("paralyzeTime")]
    [Access(typeof(ParalyzeOnCollideSystem), Other = AccessPermissions.ReadWrite)]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Whether or not to paralyze the thing we collide with
    /// </summary>
    [DataField("paralyzeOther")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ParalyzeOther = true;

    /// <summary>
    /// Whether or not to paralyze one self when colliding
    /// </summary>
    [DataField("paralyzeSelf")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ParalyzeSelf = true;
}