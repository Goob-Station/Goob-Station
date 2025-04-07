// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Buckle;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Checks if the owner is buckled or not
/// </summary>
public sealed partial class BuckledPrecondition : HTNPrecondition
{
    private SharedBuckleSystem _buckle = default!;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("isBuckled")] public bool IsBuckled = true;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _buckle = sysManager.GetEntitySystem<SharedBuckleSystem>();
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        return IsBuckled && _buckle.IsBuckled(owner) ||
               !IsBuckled && !_buckle.IsBuckled(owner);
    }
}