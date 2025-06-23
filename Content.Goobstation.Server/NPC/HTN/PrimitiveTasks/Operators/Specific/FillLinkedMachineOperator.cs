// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.Bots;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Body.Part;
using Content.Shared.DeviceLinking;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Unit;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Materials;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class FillLinkedMachineOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SharedMaterialStorageSystem _sharedMaterialStorage = default!;
    private SharedDisposalUnitSystem _sharedDisposalUnitSystem = default!;
    private SharedHandsSystem _sharedHandsSystem = default!;

    /// <summary>
    /// Target entity to inject.
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _sharedMaterialStorage = sysManager.GetEntitySystem<SharedMaterialStorageSystem>();
        _sharedDisposalUnitSystem = sysManager.GetEntitySystem<SharedDisposalUnitSystem>();
        _sharedHandsSystem = sysManager.GetEntitySystem<SharedHandsSystem>();
    }

    public override void TaskShutdown(NPCBlackboard blackboard, HTNOperatorStatus status)
    {
        base.TaskShutdown(blackboard, status);
        blackboard.Remove<EntityUid>(TargetKey);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager) || _entManager.Deleted(target)
            || !_entManager.TryGetComponent(owner, out FillbotComponent? fillbot)
            || !_entManager.HasComponent<HandsComponent>(owner)
            || !_entManager.TryGetComponent(owner, out DeviceLinkSourceComponent? fillbotlinks)
            || fillbotlinks.LinkedPorts.Count != 1
            || fillbot.LinkedSinkEntity == null
            || _entManager.Deleted(fillbot.LinkedSinkEntity))
            return HTNOperatorStatus.Failed;

        _entManager.TryGetComponent(fillbot.LinkedSinkEntity, out MaterialStorageComponent? linkedStorage);
        _entManager.TryGetComponent(fillbot.LinkedSinkEntity, out DisposalUnitComponent? disposalUnit);

        var heldItem = _sharedHandsSystem.GetActiveItem(owner);

        if (heldItem == null
            || _entManager.HasComponent<BodyPartComponent>(heldItem))
        {
            _sharedHandsSystem.TryDrop(owner);
            return HTNOperatorStatus.Failed;
        }

        if (linkedStorage is not null
            && _sharedMaterialStorage.TryInsertMaterialEntity(owner, heldItem.Value, fillbot.LinkedSinkEntity!.Value))
            return HTNOperatorStatus.Finished;

        if (disposalUnit is not null)
        {
            _sharedDisposalUnitSystem.DoInsertDisposalUnit(fillbot.LinkedSinkEntity!.Value, heldItem.Value, owner);
            return HTNOperatorStatus.Finished;
        }

        _sharedHandsSystem.TryDrop(owner);
        return HTNOperatorStatus.Failed;
    }
}
