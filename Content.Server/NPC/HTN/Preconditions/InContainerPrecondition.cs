// SPDX-FileCopyrightText: 2024 Vigers Ray <60344369+VigersRay@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Server.Containers;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Checks if the owner in container or not
/// </summary>
public sealed partial class InContainerPrecondition : HTNPrecondition
{
    private ContainerSystem _container = default!;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("isInContainer")] public bool IsInContainer = true;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _container = sysManager.GetEntitySystem<ContainerSystem>();
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        return IsInContainer && _container.IsEntityInContainer(owner) ||
               !IsInContainer && !_container.IsEntityInContainer(owner);
    }
}