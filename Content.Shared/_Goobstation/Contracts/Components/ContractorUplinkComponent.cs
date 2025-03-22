using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Contracts.Components;

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

    /// <summary>
    /// The reputation value associated with a contractor's component.
    /// </summary>
    [DataField]
    public TimeSpan ExtractionCooldown = TimeSpan.Zero;
}

[Serializable, NetSerializable]
public sealed class ContractorUplinkBoundUserInterfaceState(
    int tc,
    Dictionary<NetEntity, List<NetEntity>> contracts,
    int rep,
    NetEntity currentTarget,
    NetEntity currentExtractionPoint,
    TimeSpan extractionCooldown) : BoundUserInterfaceState
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

    public readonly TimeSpan ExtractionCooldown = extractionCooldown;
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
