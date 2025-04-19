using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.Objectives;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Spy;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SpySystem : SharedSpySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private readonly ProtoId<WeightedRandomPrototype> _weightedItemObjectives = "ThiefObjectiveGroupItem";
    private readonly ProtoId<WeightedRandomPrototype> _weightedStructureObjectives = "ThiefObjectiveGroupStructure";

    private const int GlobalBountyAmount = 10;

    public override void Initialize()
    {
        base.Initialize();
        InitializeUplink();
    }
}
