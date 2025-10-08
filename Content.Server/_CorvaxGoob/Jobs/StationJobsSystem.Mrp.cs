using System.Linq;
using Content.Server.Station.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Station.Systems;

public sealed partial class StationJobsSystem
{

    /// <summary>
    ///     Applies MRP visibility rules to the station job list.
    ///     mrpEnabled:
    ///         true  -> hide jobs with mrp == false (explicitly non-MRP)
    ///         false -> hide jobs with mrp == true  (explicitly MRP-only)
    ///         null  -> neutral, not hidden here
    /// </summary>
    public void ApplyMrpJobsFilter(EntityUid station, bool mrpEnabled)
    {
        StationJobsComponent? jobs = null;
        if (!Resolve(station, ref jobs, false))
            return;

        foreach (var jobId in jobs.JobList.Keys.ToList())
        {
            if (!_prototypeManager.TryIndex(jobId, out var proto))
                continue;

            if (proto.Mrp is null)
                continue;

            if ((mrpEnabled && proto.Mrp == false) || (!mrpEnabled && proto.Mrp == true))
                jobs.JobList.Remove(jobId);
        }

        jobs.TotalJobs = jobs.JobList.Values.Select(x => x ?? 0).Sum();
        UpdateJobsAvailable();
    }

    // Backwards-compat shim for previous callsites
    public void RemoveMrpJobs(EntityUid station)
        => ApplyMrpJobsFilter(station, false);
}
