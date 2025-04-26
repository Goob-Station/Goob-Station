using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.DoAfter;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Objectives;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Store;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Spy;

/// <summary>
/// This file sucks but I dont wanna throw these in components  yet
/// </summary>
public sealed partial class SpySystem
{
    private readonly ProtoId<WeightedRandomPrototype> _easyObjectives = "SpyBountyObjectiveGroupEasy"; // TC 0-50
    private readonly ProtoId<WeightedRandomPrototype> _mediumObjectives = "SpyBountyObjectiveGroupMedium"; // TC 50-75
    private readonly ProtoId<WeightedRandomPrototype> _hardObjectives = "SpyBountyObjectiveGroupHard"; // TC 75+

    private readonly HashSet<ProtoId<StoreCategoryPrototype>> _categories =
    [
        "UplinkWeaponry",
        "UplinkExplosives",
        "UplinkChemicals",
        "UplinkDeception",
        "UplinkDisruption",
        "UplinkImplants",
        "UplinkAllies",
        "UplinkWearables",
        "UplinkJob", // <-- funny
    ];

    private readonly (SpyBountyDifficulty Difficulty, int Weight)[] _difficultyWeights =
    [
        (SpyBountyDifficulty.Easy, 5),
        (SpyBountyDifficulty.Medium, 4),
        (SpyBountyDifficulty.Hard, 3),
    ];

    public readonly record struct StealTarget(
        EntityPrototype Proto,
        StealConditionComponent Condition,
        SpyBountyDifficulty Diff
    );

    public readonly record struct StealTargetId(
        EntProtoId Proto,
        SpyBountyDifficulty Diff
    );

    private const int GlobalBountyAmount = 10;
}
