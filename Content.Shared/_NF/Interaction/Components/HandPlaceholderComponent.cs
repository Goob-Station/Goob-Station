using Content.Shared._NF.Interaction.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Interaction.Components;

/// <summary>
/// Lets this placeholder entity "pick up" items by clicking on them.
/// The picked up item will then have <see cref="HandPlaceholderRemoveableComponent"/> added, referencing this placeholder.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(HandPlaceholderSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class HandPlaceholderComponent : Component
{
    /// <summary>
    /// A whitelist to match entities that this should accept.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// The prototype to use for placeholder icon visuals.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId? Prototype;

    /// <summary>
    /// The source entity that this placeholder gets stored in when an item is picked up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Source;

    /// <summary>
    /// The container on <see cref="Source"/> to insert this placeholder into.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ContainerId = string.Empty;

    /// <summary>
    /// Controls preventing removal from containers.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled;
}