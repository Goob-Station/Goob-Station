// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Is the specified coordinate not in range of us.
/// </summary>
public sealed partial class CoordinatesNotInRangePrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SharedTransformSystem _transformSystem = default!;

    [DataField("targetKey", required: true)] public string TargetKey = default!;

    [DataField("rangeKey", required: true)]
    public string RangeKey = default!;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _transformSystem = sysManager.GetEntitySystem<SharedTransformSystem>();
    }

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue<EntityCoordinates>(NPCBlackboard.OwnerCoordinates, out var coordinates, _entManager))
            return false;

        if (!blackboard.TryGetValue<EntityCoordinates>(TargetKey, out var target, _entManager))
            return false;

        return !_transformSystem.InRange(coordinates, target, blackboard.GetValueOrDefault<float>(RangeKey, _entManager));
    }
}
