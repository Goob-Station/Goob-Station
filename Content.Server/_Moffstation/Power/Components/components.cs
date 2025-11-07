using Content.Server._Moffstation.Power.EntitySystems;
using Content.Server.Power.Components;

namespace Content.Server._Moffstation.Power.Components;

/// <summary>
/// This component, when on an entity with <see cref="ExtensionCableReceiverComponent"/>, enables "relaying" power
/// connections it receives to entities in its containers (eg. A Blade Server Rack powering Blade Servers it contains).
/// In order for an entity to receive power in this way, the container must:
/// - have <c>InnerCableProviderComponent</c>
/// - have a container which starts with <see cref="ConnectableContainersPrefix"/>
/// - have <see cref="ExtensionCableReceiverComponent"/>
/// and the contained entity must:
/// - have <see cref="InnerCableReceiverComponent"/>
/// - be in the container with the ID mentioned above
/// - have <see cref="ExtensionCableReceiverComponent"/>
///
/// At runtime, <see cref="UnconnectableContainers"/> is used with <see cref="InnerCableSystem.SetInnerProviderContainerConnectable"/>
/// to "turn on" and "off" enabled connections at the container level.
/// </summary>
/// <seealso cref="InnerCableReceiverComponent"/>
/// <seealso cref="InnerCableSystem"/>
[RegisterComponent, Access(typeof(InnerCableSystem))]
public sealed partial class InnerCableProviderComponent : Component
{
    /// <summary>
    /// This is used to identify which containers "work with" inner cable power. Specifically, containers on this entity
    /// whose IDs match any of these strings are eligible to have their contents connect to this provider.
    /// </summary>
    [DataField(required: true)]
    public List<string> ConnectableContainers;

    /// <summary>
    /// Containers in this list are NOT able to be powered so long as they are in this list. This is used to implement
    /// dynamically enabling and disabling power to certain containers and can be thought of as a provider-level analog
    /// to <see cref="InnerCableReceiverComponent.Connectable"/>.
    /// </summary>
    [DataField]
    public List<string> UnconnectableContainers = [];

    /// <summary>
    /// The receivers currently connected to this provider.
    /// </summary>
    [ViewVariables]
    public readonly List<Entity<InnerCableReceiverComponent>> ConnectedReceivers = [];
}

/// <summary>
/// This component enables an entity to receive power relayed via a <see cref="InnerCableProviderComponent"/>. See that
/// component for detailed documentation.
/// </summary>
/// <seealso cref="InnerCableProviderComponent"/>
/// <seealso cref="InnerCableSystem"/>
[RegisterComponent, Access(typeof(InnerCableSystem))]
public sealed partial class InnerCableReceiverComponent : Component
{
    /// <summary>
    /// The provider this receiver is currently connected to.
    /// </summary>
    [ViewVariables]
    public Entity<InnerCableProviderComponent>? Provider;

    /// <summary>
    /// Whether or not this receiver should be able to connect to a provider, even if it's put into a container which
    /// would permit connection.
    /// </summary>
    [ViewVariables]
    public bool Connectable = true;
}
