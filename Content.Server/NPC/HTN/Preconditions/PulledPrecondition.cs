// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using PullingSystem = Content.Shared.Movement.Pulling.Systems.PullingSystem;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Checks if the owner is being pulled or not.
/// </summary>
public sealed partial class PulledPrecondition : HTNPrecondition
{
    private PullingSystem _pulling = default!;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("isPulled")] public bool IsPulled = true;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _pulling = sysManager.GetEntitySystem<PullingSystem>();
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        return IsPulled && _pulling.IsPulled(owner) ||
               !IsPulled && !_pulling.IsPulled(owner);
    }
}