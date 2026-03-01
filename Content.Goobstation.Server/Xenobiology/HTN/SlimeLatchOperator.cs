// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;

namespace Content.Goobstation.Server.Xenobiology.HTN;

public sealed partial class SlimeLatchOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SlimeLatchSystem _slimeLatch = default!;

    [DataField]
    public string LatchKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _slimeLatch = sysManager.GetEntitySystem<SlimeLatchSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        base.Update(blackboard, frameTime);

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(LatchKey);

        if (!_entManager.TryGetComponent<SlimeComponent>(owner, out var slime))
            return HTNOperatorStatus.Failed;

        if (_slimeLatch.IsLatched((owner, slime), target))
            return HTNOperatorStatus.Finished;

        if (_entManager.HasComponent<BeingLatchedComponent>(target))
            return HTNOperatorStatus.Continuing;

        return _slimeLatch.NpcTryLatch((owner, slime), target)
            ? HTNOperatorStatus.Continuing
            : HTNOperatorStatus.Failed;
    }
}
