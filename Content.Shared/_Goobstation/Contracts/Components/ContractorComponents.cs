using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Contracts.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContractorComponent : Component
{
    /// <summary>
    /// Represents a collection of contracts within the contractor component,
    /// where each contract maps a <see cref="NetEntity"/> to a corresponding list of <see cref="NetEntity"/>.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public Dictionary<NetEntity, List<NetEntity>> Contracts = new(5);

    /// <summary>
    /// The current target entity for the contractor functionality.
    /// This entity identifier is networked and auto-managed to ensure consistent
    /// synchronization across the client and server.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public NetEntity CurrentTarget = NetEntity.Invalid;

    /// <summary>
    /// The current extraction point in a contractor system.
    /// </summary>
    /// <value>
    /// The value is of type <see cref="NetEntity"/> and defaults to <c>NetEntity.Invalid</c>
    /// when not set.
    /// </value>
    [DataField]
    [AutoNetworkedField]
    public NetEntity CurrentExtractionPoint = NetEntity.Invalid;

    /// <summary>
    /// Represents the number of telecrystals (Tc) awarded as a reward for completing a contract.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public int TcReward;

    /// <summary>
    /// A variable representing a contractor component's currency or transactional value
    /// in the system.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public int Tc;

    /// <summary>
    /// The total Telecrystals (Tc) available within the contractor component.
    /// This value is utilized to track the accumulated Tc, which can influence
    /// gameplay mechanics or associated functionality related to contracts.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public int TotalTc;

    /// <summary>
    /// The reputation value associated with a contractor's component.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public int Rep;

    /// <summary>
    /// How long to wait before searching for new contracts
    /// </summary>
    public TimeSpan RefreshTime = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long to wait before searching for new contracts
    /// </summary>
    [DataField]
    public TimeSpan TryAgainTime = TimeSpan.Zero;
}

/// <summary>
/// A marker component used to represent specific contractor-related points of interest in the game world.
/// This component includes data for a location's name and the Tactical Credits (TC) reward associated with it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContractorMarkerComponent : Component
{
    /// <summary>
    /// The name of the contractor marker, identified using a localized identifier (<see cref="LocId"/>).
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId? Name;

    /// <summary>
    /// The Telecrystals (TC) reward associated with a specific contractor marker in the contractor system.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TcReward;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContractorPortalComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Target = EntityUid.Invalid;

    [DataField, AutoNetworkedField]
    public bool Used = false;
}

[Serializable, NetSerializable]
public sealed partial class ExtractionDoAfterEvent : SimpleDoAfterEvent;

[RegisterComponent]
public sealed partial class ContractorWarpMarkerComponent : Component;

[RegisterComponent]
public sealed partial class ContractorPrisonerComponent : Component
{
    [DataField]
    public TimeSpan TimeLeft = TimeSpan.Zero;

    [DataField]
    public EntityCoordinates ReturnCoordinates;

    [DataField]
    public EntityUid Gear = EntityUid.Invalid;

    public TimeSpan PrisonerTime = TimeSpan.FromSeconds(10); // bump up to prod value

    [DataField]
    public bool ReturnPortal = false;
}
