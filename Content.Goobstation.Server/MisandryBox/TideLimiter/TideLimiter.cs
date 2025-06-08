using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Server.MisandryBox.TideLimiter;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationInitializedEvent>(OnInitialized);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnAssigned);
    }

    private void OnInitialized(StationInitializedEvent ev)
    {
        // The lion respects the bodily autonomy of the small grid when it initializes
        if (!HasComp<StationJobsComponent>(ev.Station))
            return;

        EnsureComp<TideLimitedComponent>(ev.Station);
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId is null)
            return;

        var comp = Comp<TideLimitedComponent>(ev.Station);

        if (IsSecurity(ev.JobId))
            IncrementRecord(ev.Station, false, comp);
        else if (ev.JobId == comp.Bound)
            IncrementRecord(ev.Station, true, comp);

        if (ev.LateJoin)
            HandleLatejoin(ev);
    }

    private void OnAssigned(RulePlayerJobsAssignedEvent _)
    {
        UpdateSlots();
        Lock();
    }

    private void HandleLatejoin(PlayerSpawnCompleteEvent _)
    {
        UpdateSlots();
    }

    private void IncrementRecord(EntityUid station, bool bound, TideLimitedComponent comp)
    {
        if (bound)
            comp.BoundCount++;
        else
            comp.SecurityCount++;

        UpdateSlots();
    }

    private void Lock()
    {
        var eqe = EntityQueryEnumerator<TideLimitedComponent>();

        while (eqe.MoveNext(out var comp))
        {
            comp.Active = true;
        }
    }

    private void UpdateSlots()
    {
        var eqe = EntityQueryEnumerator<TideLimitedComponent>();

        while (eqe.MoveNext(out var ent, out var comp))
        {
            var slots = GetBoundAvailableSlots(comp);

            _stationJob.TrySetJobSlot(ent, comp.Bound, slots);
        }
    }

    private int GetBoundAvailableSlots(TideLimitedComponent comp)
    {
        return Math.Min(0, comp.BoundCount - comp.SecurityCount * comp.Ratio);
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
                if (comps.Value.Component is SecurityStaffComponent) // This entrenches into heretic therefore funny.
                    return true;
            }
        }

        return false;
    }
}
