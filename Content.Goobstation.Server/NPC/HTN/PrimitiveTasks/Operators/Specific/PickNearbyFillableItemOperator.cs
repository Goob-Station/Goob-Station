// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Silicon.Bots;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Body.Part;
using Content.Shared.DeviceLinking;
using Content.Shared.Disposal.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class PickNearbyFillableItemOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    private EntityWhitelistSystem _whitelistSystem = default!;
    private SharedMaterialStorageSystem _sharedMaterialStorage = default!;
    private EntityLookupSystem _lookup = default!;
    private PathfindingSystem _pathfinding = default!;
    private SharedHandsSystem _sharedHandsSystem = default!;
    private TagSystem _tagSystem = default!;

    [DataField] public string RangeKey = NPCBlackboard.FillbotPickupRange;

    /// Target entity to pick up
    [DataField(required: true)]
    public string TargetKey = string.Empty;

    /// Target entitycoordinates to move to.
    [DataField(required: true)]
    public string TargetMoveKey = string.Empty;

    /// Target tag prototype to look for when trying to find trash.
    [DataField]
    public ProtoId<TagPrototype> TrashProto = "Trash";


    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _whitelistSystem = sysManager.GetEntitySystem<EntityWhitelistSystem>();
        _lookup = sysManager.GetEntitySystem<EntityLookupSystem>();
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
        _sharedMaterialStorage = sysManager.GetEntitySystem<SharedMaterialStorageSystem>();
        _sharedHandsSystem = sysManager.GetEntitySystem<SharedHandsSystem>();
        _tagSystem = sysManager.GetEntitySystem<TagSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _entManager)
            || !_entManager.TryGetComponent<FillbotComponent>(owner, out var fillbot)
            || !_entManager.TryGetComponent<DeviceLinkSourceComponent>(owner, out var fillbotlinks)
            || fillbotlinks.LinkedPorts.Count != 1
            || fillbot.LinkedSinkEntity == null)
            return (false, null);

        _entManager.TryGetComponent(fillbot.LinkedSinkEntity, out MaterialStorageComponent? linkedStorage);
        _entManager.TryGetComponent(fillbot.LinkedSinkEntity, out DisposalUnitComponent? disposalUnit);

        foreach (var target in _lookup.GetEntitiesInRange(owner, range))
        {
            // only things the robot can actually pick up
            if (!_sharedHandsSystem.CanPickupAnyHand(owner, target))
                continue;

            // only things not currently contained by something else
            if (_entManager.TryGetComponent<MetaDataComponent>(target, out var meta) &&
                meta.Flags.HasFlag(MetaDataFlags.InContainer))
                continue;

            // only things that can go inside
            if (linkedStorage != null && !_sharedMaterialStorage.CanInsertMaterialEntity(target, fillbot.LinkedSinkEntity!.Value))
                continue;

            // trash only
            if (disposalUnit != null &&
                (_whitelistSystem.IsWhitelistFail(disposalUnit.Whitelist, target)
                    || !_tagSystem.HasTag(target, TrashProto)
                    || _entManager.HasComponent<BodyPartComponent>(target))) // Robot is unable to insert bodyparts into Disposals for some reason
                continue;

            const float pathRange = SharedInteractionSystem.InteractionRange - 1;
            var path = await _pathfinding.GetPath(owner, target, pathRange, cancelToken);

            if (path.Result == PathResult.NoPath)
                continue;

            return (true, new()
            {
                {TargetKey, target},
                {TargetMoveKey, _entManager.GetComponent<TransformComponent>(target).Coordinates},
                {NPCBlackboard.PathfindKey, path},
            });
        }

        return (false, null);
    }
}
