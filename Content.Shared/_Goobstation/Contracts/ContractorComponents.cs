using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Contracts;

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
    /// The reputation value associated with a contractor's component.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public TimeSpan ExtractionCooldown = TimeSpan.Zero;
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

/// <summary>
/// Represents the state of the Contractor Uplink Bound User Interface, encapsulating information related to telecrystals, contracts, reputation,
/// the current contract target, and the current extraction point.
/// </summary>
[Serializable, NetSerializable]
public sealed class ContractorUplinkBoundUserInterfaceState(
    int tc,
    Dictionary<NetEntity, List<NetEntity>> contracts,
    int rep,
    NetEntity currentTarget,
    NetEntity currentExtractionPoint) : BoundUserInterfaceState
{
    /// <summary>
    /// Telecrystal (Tc) count available to the contractor.
    /// </summary>
    public readonly int Tc = tc;

    /// <summary>
    /// The reputation or standing of the contractor within the contractor system.
    /// </summary>
    public readonly int Rep = rep;

    /// <summary>
    /// The currently selected target in the contractor uplink interface,
    /// tying a specific <see cref="NetEntity"/> to operations related to contracts or objectives.
    /// </summary>
    public readonly NetEntity CurrentTarget = currentTarget;

    /// <summary>
    /// Represents the networked identifier of the current extraction point
    /// in the contractor gameplay component.
    /// </summary>
    public readonly NetEntity CurrentExtractionPoint = currentExtractionPoint;

    /// <summary>
    /// Represents a mapping of entities to their associated contract entities, utilized in the contractor component.
    /// </summary>
    public readonly Dictionary<NetEntity, List<NetEntity>> Contracts = contracts;
}

[Serializable, NetSerializable]
public enum ContractorUplinkUiKey
{
    Key,
}

[Serializable, NetSerializable]
public enum UiMessage
{
    SelectTarget,

    Refresh,

    TryExtraction
}

[Serializable, NetSerializable]
public sealed class ContractorUiMessage(UiMessage button, NetEntity target, NetEntity location) : BoundUserInterfaceMessage
{
    public readonly UiMessage Button = button;

    public readonly NetEntity Target = target;

    public readonly NetEntity Location = location;
}

/// <summary>
/// Used for handling contractor uplink functionality,
/// including tracking usage, managing portal spawn timing, and associated items or actions.
/// </summary>
[RegisterComponent, NetworkedComponent ,AutoGenerateComponentState]
public sealed partial class ContractorUplinkComponent : Component
{
    /// <summary>
    /// Indicates whether the contractor uplink component has been utilized.
    /// </summary>
    [DataField, AutoNetworkedField]
    public NetEntity User = NetEntity.Invalid;

    /// <summary>
    /// The EntProtoId for the contractor flare entity used within the ContractorUplinkComponent.
    /// </summary>
    /// <remarks>/// </remarks>
    public EntProtoId Flare = "ContractorFlare";

    /// <summary>
    /// Represents the duration of time to wait before a portal spawns
    /// after an activation or triggering event in the contractor uplink component.
    /// </summary>
    public TimeSpan PortalSpawnTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Represents the duration of time to wait before a portal spawns
    /// after an activation or triggering event in the contractor uplink component.
    /// </summary>
    public NetEntity FlareUid = NetEntity.Invalid;

    /// <summary>
    /// Tracks the remaining time until the contractor portal spawns,
    /// allowing dynamic adjustments or resets during gameplay.
    /// </summary>
    public TimeSpan PortalSpawnTimer = TimeSpan.Zero;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContractorPortalComponent : Component
{
    [DataField, AutoNetworkedField]
    public NetEntity LinkedUplink = NetEntity.Invalid;

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
}
