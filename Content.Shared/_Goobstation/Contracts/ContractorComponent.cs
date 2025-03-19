using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
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
    /// <remarks>
    /// This integer value is utilized to track and manage the reputation of a contractor within the system.
    /// It is synced over the network to ensure consistent data across clients and the server.
    /// </remarks>
    [DataField]
    [AutoNetworkedField]
    public int Rep;

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
    /// <remarks>
    /// This property is serialized and networked for use across both client and server environments, ensuring synchronization in localized contexts.
    /// A null value indicates the absence of a specified name.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public LocId? Name;

    /// <summary>
    /// The Telecrystals (TC) reward associated with a specific contractor marker in the contractor system.
    /// </summary>
    /// <remarks>
    /// This value is stored as an integer and is networked across the system for usage in gameplay mechanics.
    /// It facilitates communication of reward details for specific contract objectives in multiplayer environments.
    /// </remarks>
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
    /// <remarks>
    /// This value is integral to the contractor's operations, serving as a resource for managing contracts and acquiring services or equipment.
    /// It is transmitted across the network to ensure synchronization in multiplayer scenarios.
    /// </remarks>
    public readonly int Tc = tc;

    /// <summary>
    /// The reputation or standing of the contractor within the contractor system.
    /// </summary>
    /// <remarks>
    /// This integer value is used to track and reflect the contractor's performance, credibility, or influence in various gameplay scenarios.
    /// It is networked and synchronized across client and server to ensure consistency in multiplayer environments.
    /// </remarks>
    public readonly int Rep = rep;

    /// <summary>
    /// The currently selected target in the contractor uplink interface,
    /// tying a specific <see cref="NetEntity"/> to operations related to contracts or objectives.
    /// </summary>
    /// <remarks>
    /// This property is used in gameplay mechanics to determine the entity that is being actively targeted
    /// for a contract or interaction within the contractor system. It is serialized for network communication
    /// and thus can be synchronized across server and client states.
    /// </remarks>
    public readonly NetEntity CurrentTarget = currentTarget;

    /// <summary>
    /// Represents the networked identifier of the current extraction point
    /// in the contractor gameplay component.
    /// </summary>
    /// <remarks>
    /// This property serves as a reference to a <see cref="NetEntity"/> that
    /// indicates the location where the current extraction operation is targeted.
    /// It is synchronized between the client and server for consistent gameplay behavior.
    /// </remarks>
    public readonly NetEntity CurrentExtractionPoint = currentExtractionPoint;

    /// <summary>
    /// Represents a mapping of entities to their associated contract entities, utilized in the contractor component.
    /// </summary>
    /// <remarks>
    /// The dictionary maps a <see cref="NetEntity"/> to a collection of <see cref="NetEntity"/> objects,
    /// indicating the relationship between contract targets and their respective extraction-related points or entities.
    /// Primarily used for managing contract assignments within a networked gameplay context.
    /// </remarks>
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
    /// <remarks>
    /// This boolean variable is networked to synchronize its state across the server and clients,
    /// allowing for consistent tracking of the component's usage status in networked gameplay contexts.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool Used;

    /// <summary>
    /// The EntProtoId for the contractor flare entity used within the ContractorUplinkComponent.
    /// </summary>
    /// <remarks>/// </remarks>
    public EntProtoId Flare = "ContractorFlare";

    /// <summary>
    /// Represents the duration of time to wait before a portal spawns
    /// after an activation or triggering event in the contractor uplink component.
    /// </summary>
    /// <remarks>/// </remarks>
    public TimeSpan PortalSpawnTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Represents the duration of time to wait before a portal spawns
    /// after an activation or triggering event in the contractor uplink component.
    /// </summary>
    /// <remarks>/// </remarks>
    public EntityUid FlareUid = EntityUid.Invalid;

    /// <summary>
    /// Tracks the remaining time until the contractor portal spawns,
    /// allowing dynamic adjustments or resets during gameplay.
    /// </summary>
    /// <remarks>/// </remarks>
    public TimeSpan PortalSpawnTimer = TimeSpan.Zero;
}


[Serializable, NetSerializable]
public sealed partial class ExtractionDoAfterEvent : SimpleDoAfterEvent;
