// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Map;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Is the specified key within the specified range of us.
/// </summary>
public sealed partial class TargetInRangePrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SharedTransformSystem _transformSystem = default!;
    private StealthSystem _stealth = default!; // goob edit

    [DataField("targetKey", required: true)] public string TargetKey = default!;

    [DataField("rangeKey", required: true)]
    public string RangeKey = default!;
    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _transformSystem = sysManager.GetEntitySystem<SharedTransformSystem>();
        _stealth = sysManager.GetEntitySystem<StealthSystem>(); // goob edit
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue<EntityCoordinates>(NPCBlackboard.OwnerCoordinates, out var coordinates, _entManager))
            return false;

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
        || !_entManager.TryGetComponent<TransformComponent>(target, out var targetXform)
        // goob edit - stealthed entities can't be seen by npcs
        || (_entManager.TryGetComponent<StealthComponent>(target, out var stealth) && _stealth.GetVisibility(target, stealth) <= stealth.ExamineThreshold))
            return false;



        var transformSystem = _entManager.System<SharedTransformSystem>;
        return _transformSystem.InRange(coordinates, targetXform.Coordinates, blackboard.GetValueOrDefault<float>(RangeKey, _entManager));
    }
}
