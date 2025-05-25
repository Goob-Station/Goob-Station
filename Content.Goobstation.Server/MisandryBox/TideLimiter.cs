using System.Diagnostics.CodeAnalysis;
using Content.Server.GameTicking;
using Content.Server.Heretic.Components;
using Content.Server.Jobs;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.MisandryBox;

/// <summary>
/// Limits available passenger job slots depending on taken security slots
/// </summary>
public sealed partial class TideLimiterSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly StationJobsSystem _stationJob = default!;

    /// <summary>
    /// Role id that is bound by this system
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    private const string Bound = "Passenger";

    [ViewVariables(VVAccess.ReadOnly)]
    private Entity<TideLimiterComponent>? _ent;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationInitializedEvent>(OnInitialized);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(Cleanup);
    }

    private void OnInitialized(StationInitializedEvent ev)
    {
        // The lion respects the bodily autonomy of the small grid when it initializes
        if (!HasComp<StationJobsComponent>(ev.Station))
            return;

        _ent = GetOrCreateHolder();

        var station = new StationSecurity(ev.Station);
        _ent.Value.Comp.Security.Add(station);
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId is null)
            return;

        if (IsSecurity(ev.JobId))
            IncrementRecord(ev.Station, false);
        else if (ev.JobId == Bound)
            IncrementRecord(ev.Station, true);

        if (ev.LateJoin)
            HandleLatejoin(ev);
    }

    private void OnAssigned(RulePlayerJobsAssignedEvent ev)
    {
        Lock();
    }

    private void HandleLatejoin(PlayerSpawnCompleteEvent ev)
    {
        UpdateSlots(ev.Station);
    }

    private void IncrementRecord(EntityUid station, bool bound)
    {
        if (!TryGetSecurityCounts(station, out var data))
            return;

        if (bound)
            data.IncrementBound();
        else
            data.IncrementSecurity();
    }

    private void Lock()
    {
        GetOrCreateHolder().Comp.Locked = true;
        UpdateSlots();
    }

    private void UpdateSlots(EntityUid? station = null)
    {
        if (station == null)
        {
            foreach (var securityRecord in GetOrCreateHolder().Comp.Security)
            {
                SetBoundRoleLimit(securityRecord);
            }

            return;
        }

        if (TryGetSecurityCounts(station.Value, out var securityRecords))
        {
            SetBoundRoleLimit(securityRecords);
        }
    }

    private bool IsSecurity(string id)
    {
        var proto = _prototypeManager.Index<JobPrototype>(id);
        foreach (var special in proto.Special)
        {
            if (special is not AddComponentSpecial addspecial)
                continue;

            foreach (var comps in addspecial.Components)
            {
                if (comps.Value.Component is SecurityStaffComponent staff) // This entrenches into heretic therefore funny.
                    return true;
            }
        }

        return false;
    }

    private void SetBoundRoleLimit(StationSecurity securityRecord)
    {
        var ratio = GetOrCreateHolder().Comp.Ratio;

        var security = securityRecord.SecurityPlayers;
        var bound = securityRecord.Bound;

        var slots = Math.Max(0, security * ratio - bound);

        _stationJob.TrySetJobSlot(securityRecord.Station, Bound, slots);
    }

    public bool TryGetSecurityCounts(EntityUid station, [NotNullWhen(true)] out StationSecurity? stationSecurity)
    {
        stationSecurity = null;
        var holder = GetOrCreateHolder();

        foreach (var record in holder.Comp.Security)
        {
            if (record.Station.Id != station.Id)
                continue;

            stationSecurity = record;
            return true;
        }

        return false;
    }

    private void Cleanup(RoundRestartCleanupEvent ev)
    {
        _ent = null;
    }

    public Entity<TideLimiterComponent> GetOrCreateHolder()
    {
        if (_ent is not null)
            return _ent.Value;

        var ent = Spawn(null, MapCoordinates.Nullspace);
        var comp = AddComp<TideLimiterComponent>(ent);

        return (ent, comp);
    }

    public bool Locked()
    {
        return GetOrCreateHolder().Comp.Locked;
    }
}

[RegisterComponent]
public sealed partial class TideLimiterComponent : Component
{
    /// <summary>
    /// Ratio of players of bound role for each security player.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int Ratio = 2;

    /// <summary>
    /// How much security do we have
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<StationSecurity> Security = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public bool Locked;
}

/// <summary>
/// Class for an amount of security on station
/// </summary>
/// <param name="station">Station entituid</param>
public sealed class StationSecurity(EntityUid station)
{
    public EntityUid Station { get; } = station;
    public int SecurityPlayers { get; private set; } = 0;
    public int Bound { get; private set; } = 0;

    public void IncrementSecurity() => SecurityPlayers++;
    public void IncrementBound() => Bound++;
}
