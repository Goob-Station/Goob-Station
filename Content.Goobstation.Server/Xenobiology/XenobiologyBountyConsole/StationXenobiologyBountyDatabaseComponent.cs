// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.Xenobiology.XenobiologyBountyConsole;

/// <summary>
/// Stores all active cargo bounties for a particular station.
/// </summary>
[RegisterComponent]
public sealed partial class StationXenobiologyBountyDatabaseComponent : Component
{
    /// <summary>
    /// A list of all the bounties currently active for a station.
    /// </summary>
    [DataField]
    public List<XenobiologyBountyData> Bounties = [];

    /// <summary>
    /// A list of all the bounties that have been completed or
    /// skipped for a station.
    /// </summary>
    [DataField]
    public List<XenobiologyBountyHistoryData> History = [];

    /// <summary>
    /// Used to determine unique order IDs
    /// </summary>
    [DataField]
    public int TotalBounties;

    /// <summary>
    /// A list of bounty IDs that have been checked this tick.
    /// Used to prevent multiplying bounty prices.
    /// </summary>
    [DataField]
    public HashSet<string> CheckedBounties = new();

    /// <summary>
    /// The group that bounties are pulled from.
    /// </summary>
    [DataField]
    public ProtoId<CargoBountyGroupPrototype> Group = "StationBounty";

    /// <summary>
    /// The time at which players will be able to skip the next bounty.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextSkipTime = TimeSpan.Zero;

    /// <summary>
    /// The time between skipping bounties.
    /// </summary>
    [DataField]
    public TimeSpan SkipDelay = TimeSpan.FromMinutes(10);

    /// <summary>
    /// The time between global bounty refreshes.
    /// </summary>
    [DataField]
    public TimeSpan GlobalMarketRefreshDelay = TimeSpan.FromMinutes(12);

    /// <summary>
    /// The time at which all bounties will refresh.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextGlobalMarketRefresh = TimeSpan.Zero;
}
