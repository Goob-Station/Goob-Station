// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Changeling.Objectives.Systems;
using Content.Server.Objectives.Components;

namespace Content.Goobstation.Server.Changeling.Objectives.Components;

/// <summary>
/// Requires that you have the same identity a target for a certain length of time before the round ends.
/// Obviously the agent id will work for this, but it's assumed that you will kill the target to prevent suspicion.
/// Depends on <see cref="TargetObjectiveComponent"/> to function.
/// </summary>
[RegisterComponent, Access(typeof(ImpersonateConditionSystem))]
public sealed partial class ImpersonateConditionComponent : Component
{
    /// <summary>
    /// Name that must match your identity for greentext.
    /// This is stored once after the objective is assigned:
    /// 1. to be a tiny bit more efficient
    /// 2. to prevent the name possibly changing when borging or anything else and messing you up
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string? Name;

    /// <summary>
    /// Mind this objective got assigned to, used to continiously checkd impersonation.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? MindId;

    public bool Completed = false;
}