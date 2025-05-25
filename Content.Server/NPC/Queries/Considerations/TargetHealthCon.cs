// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs;

namespace Content.Server.NPC.Queries.Considerations;

/// <summary>
/// Goes linearly from 1f to 0f, with 0 damage returning 1f and <see cref=TargetState> damage returning 0f
/// </summary>
public sealed partial class TargetHealthCon : UtilityConsideration
{

    /// <summary>
    /// Which MobState the consideration returns 0f at, defaults to choosing earliest incapacitating MobState
    /// </summary>
    [DataField("targetState")]
    public MobState TargetState = MobState.Invalid;
}