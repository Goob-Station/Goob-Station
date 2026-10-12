using Content.Goobstation.Shared.EntityConditions;
using Content.Server.Instruments;
using Content.Server.Objectives.Components;
using Content.Server.Power.Components;
using Content.Shared.Mind;
using Content.Shared.Shuttles.Components;
using Content.Shared.Stacks;
using Content.Shared.Station;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class PeriodicRequirementsCondition : AchievementCondition;

public sealed partial class CarryingStackCondition : AchievementCondition
{
    [DataField(required: true)]
    public ProtoId<StackPrototype> Stack;

    [DataField(required: true)]
    public int Amount;
}

public sealed partial class DistanceFromStationCondition : AchievementCondition
{
    [DataField(required: true)]
    public float Distance;
}

public sealed partial class LostInFtlCondition : AchievementCondition
{
    [DataField]
    public int Checks = 3;
}

public sealed partial class PowerSupplyCondition : AchievementCondition
{
    [DataField(required: true)]
    public float Watts;
}

public sealed partial class BandCondition : AchievementCondition
{
    [DataField]
    public int MinSize = 2;

    [DataField]
    public bool RequireTwin;
}

public abstract partial class PeriodicConditionSystem<T> : AchievementConditionSystem<T> where T : AchievementCondition
{
    [Dependency] protected IGameTiming Timing = default!;
    [Dependency] protected IPlayerManager PlayerManager = default!;

    protected virtual TimeSpan Interval => TimeSpan.FromSeconds(5);

    private TimeSpan _nextCheck;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (Entries.Count == 0 || Timing.CurTime < _nextCheck)
            return;

        _nextCheck = Timing.CurTime + Interval;
        Check();
    }

    protected abstract void Check();
}

public sealed partial class PeriodicRequirementsConditionSystem : PeriodicConditionSystem<PeriodicRequirementsCondition>
{
    protected override void Check()
    {
        foreach (var session in PlayerManager.Sessions)
        {
            if (session.AttachedEntity is { } player)
                ProgressAll(player);
        }
    }
}

public sealed partial class CarryingStackConditionSystem : PeriodicConditionSystem<CarryingStackCondition>
{
    [Dependency] private CarriedItemsSystem _carried = default!;

    protected override void Check()
    {
        foreach (var session in PlayerManager.Sessions)
        {
            if (session.AttachedEntity is not { } player)
                continue;

            var items = _carried.GetCarriedItems(player);
            foreach (var (proto, condition) in Entries)
            {
                var total = 0;
                foreach (var item in items)
                {
                    if (TryComp<StackComponent>(item, out var stack) && stack.StackTypeId == condition.Stack)
                        total += stack.Count;
                }

                if (total >= condition.Amount)
                    Achievements.AddProgress(player, proto);
            }
        }
    }
}

public sealed partial class DistanceFromStationConditionSystem : PeriodicConditionSystem<DistanceFromStationCondition>
{
    [Dependency] private SharedStationSystem _station = default!;
    [Dependency] private TransformSystem _transform = default!;

    protected override void Check()
    {
        var grids = new List<MapCoordinates>();
        foreach (var station in _station.GetStations())
        {
            if (_station.GetLargestGrid(station) is { } grid)
                grids.Add(_transform.GetMapCoordinates(grid));
        }

        if (grids.Count == 0)
            return;

        foreach (var session in PlayerManager.Sessions)
        {
            if (session.AttachedEntity is not { } player)
                continue;

            var position = _transform.GetMapCoordinates(player);
            var closest = float.MaxValue;
            foreach (var grid in grids)
            {
                if (grid.MapId == position.MapId)
                    closest = MathF.Min(closest, (grid.Position - position.Position).Length());
            }

            if (closest == float.MaxValue)
                continue;

            foreach (var (proto, condition) in Entries)
            {
                if (closest >= condition.Distance)
                    Achievements.AddProgress(player, proto);
            }
        }
    }
}

public sealed partial class LostInFtlConditionSystem : PeriodicConditionSystem<LostInFtlCondition>
{
    private readonly Dictionary<EntityUid, int> _drifting = new();

    protected override void Check()
    {
        var seen = new HashSet<EntityUid>();
        foreach (var session in PlayerManager.Sessions)
        {
            if (session.AttachedEntity is not { } player)
                continue;

            var xform = Transform(player);
            if (xform.GridUid != null
                || xform.MapUid is not { } map
                || xform.ParentUid != map
                || !HasComp<FTLMapComponent>(map))
                continue;

            seen.Add(player);
            var count = _drifting.GetValueOrDefault(player) + 1;
            _drifting[player] = count;

            foreach (var (proto, condition) in Entries)
            {
                if (count == condition.Checks)
                    Achievements.AddProgress(player, proto);
            }
        }

        foreach (var uid in new List<EntityUid>(_drifting.Keys))
        {
            if (!seen.Contains(uid))
                _drifting.Remove(uid);
        }
    }
}

public sealed partial class PowerSupplyConditionSystem : PeriodicConditionSystem<PowerSupplyCondition>
{
    protected override TimeSpan Interval => TimeSpan.FromSeconds(30);

    protected override void Check()
    {
        var supply = new Dictionary<EntityUid, float>();
        var query = EntityQueryEnumerator<PowerSupplierComponent, TransformComponent>();
        while (query.MoveNext(out _, out var supplier, out var xform))
        {
            if (xform.GridUid is { } grid)
                supply[grid] = supply.GetValueOrDefault(grid) + supplier.CurrentSupply;
        }

        foreach (var session in PlayerManager.Sessions)
        {
            if (session.AttachedEntity is not { } player || Transform(player).GridUid is not { } grid)
                continue;

            var gridSupply = supply.GetValueOrDefault(grid);
            foreach (var (proto, condition) in Entries)
            {
                if (gridSupply >= condition.Watts)
                    Achievements.AddProgress(player, proto);
            }
        }
    }
}

public sealed partial class BandConditionSystem : PeriodicConditionSystem<BandCondition>
{
    protected override TimeSpan Interval => TimeSpan.FromSeconds(10);

    protected override void Check()
    {
        var bands = new Dictionary<EntityUid, HashSet<EntityUid>>();
        var query = EntityQueryEnumerator<InstrumentComponent>();
        while (query.MoveNext(out var uid, out var instrument))
        {
            if (!instrument.Playing || instrument.Master is not { } master || instrument.InstrumentPlayer is not { } player)
                continue;

            if (!bands.TryGetValue(master, out var members))
            {
                members = new HashSet<EntityUid>();
                bands[master] = members;

                if (TryComp<InstrumentComponent>(master, out var masterInstrument) && masterInstrument.InstrumentPlayer is { } leader)
                    members.Add(leader);
            }

            members.Add(player);
        }

        foreach (var members in bands.Values)
        {
            foreach (var (proto, condition) in Entries)
            {
                if (members.Count < condition.MinSize)
                    continue;

                foreach (var player in members)
                {
                    if (condition.RequireTwin && !HasTwinInBand(player, members))
                        continue;

                    Achievements.AddProgress(player, proto);
                }
            }
        }
    }

    private bool HasTwinInBand(EntityUid player, HashSet<EntityUid> members)
    {
        return TryComp<TargetOverrideComponent>(player, out var target)
               && TryComp<MindComponent>(target.Target, out var mind)
               && mind.OwnedEntity is { } original
               && members.Contains(original);
    }
}
