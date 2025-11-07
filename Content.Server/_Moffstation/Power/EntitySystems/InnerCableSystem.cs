using System.Linq;
using Content.Server._Moffstation.Power.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._Moffstation.Power.EntitySystems;

/// <summary>
/// This system implements <see cref="InnerCableProviderComponent"/>.
/// </summary>
public sealed partial class InnerCableSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InnerCableProviderComponent, ComponentStartup>(OnInnerProviderStarted);
        SubscribeLocalEvent<InnerCableProviderComponent, ComponentShutdown>(OnInnerProviderShutdown);
        SubscribeLocalEvent<InnerCableReceiverComponent, ComponentStartup>(OnInnerReceiverStarted);
        SubscribeLocalEvent<InnerCableReceiverComponent, ComponentShutdown>(OnInnerReceiverShutdown);

        SubscribeLocalEvent<InnerCableProviderComponent, ExtensionCableSystem.ProviderConnectedEvent>(
            OnInnerProviderConnected);
        SubscribeLocalEvent<InnerCableProviderComponent, ExtensionCableSystem.ProviderDisconnectedEvent>(
            OnInnerProviderDisconnected);

        SubscribeLocalEvent<InnerCableProviderComponent, EntInsertedIntoContainerMessage>(OnInsertedIntoInnerProvider);
        SubscribeLocalEvent<InnerCableProviderComponent, EntRemovedFromContainerMessage>(OnRemovedFromInnerProvider);
    }

    #region public

    /// <summary>
    /// Enables or disables inner-cable connections to this receiver. If this receiver is in a container when the
    /// connectability is changed, the receiver will dis/connect from/to the container as possible.
    /// </summary>
    public void SetInnerReceiverConnectable(Entity<InnerCableReceiverComponent?> receiverEntity, bool connectable)
    {
        if (ResolveOrNull(receiverEntity) is not { } receiver ||
            receiver.Comp.Connectable == connectable)
            return;

        receiver.Comp.Connectable = connectable;

        if (receiver.Comp.Connectable)
        {
            TryConnectToContainingProvider(receiver);
        }
        else
        {
            DisconnectFromContainingProvider(receiver);
        }
    }

    /// <summary>
    /// Enables or disables inner-cable connections to receivers in the <paramref name="containerId">container</paramref>.
    /// If the container contains <see cref="InnerCableReceiverComponent">receivers</see> when the connectability is
    /// changed, those receivers will dis/connect from/to this provider as possible.
    /// If no container with the given ID exists, this function does nothing.
    /// </summary>
    public void SetInnerProviderContainerConnectable(
        Entity<InnerCableProviderComponent?> providerEntity,
        string containerId,
        bool connectable
    )
    {
        if (ResolveOrNull(providerEntity) is not { } provider ||
            !provider.Comp.ConnectableContainers.Contains(containerId) ||
            !_container.TryGetContainer(provider, containerId, out var container) ||
            connectable == !provider.Comp.UnconnectableContainers.Contains(containerId))
            return;

        var outerProvider = GetOuterProviderOrNull(provider);
        if (connectable)
        {
            provider.Comp.UnconnectableContainers.Remove(containerId);
            foreach (var entity in container.ContainedEntities)
            {
                if (TryComp<InnerCableReceiverComponent>(entity) is not { } receiver ||
                    !receiver.Comp.Connectable)
                    continue;

                ForceConnectInnerCable(provider, receiver, outerProvider);
            }
        }
        else
        {
            provider.Comp.UnconnectableContainers.Add(containerId);
            foreach (var entity in container.ContainedEntities)
            {
                if (TryComp<InnerCableReceiverComponent>(entity) is not { } receiver)
                    continue;

                DisconnectInnerCable(provider, receiver, outerProvider);
            }
        }
    }

    #endregion

    #region Provider

    private void OnInnerProviderStarted(Entity<InnerCableProviderComponent> provider, ref ComponentStartup args)
    {
        var outerProvider = GetOuterProviderOrNull(provider);

        foreach (var container in _container.GetAllContainers(provider))
        {
            if (!IsContainerConnectable(provider, container))
                continue;

            foreach (var entity in container.ContainedEntities)
            {
                if (TryComp<InnerCableReceiverComponent>(entity) is not { } receiver ||
                    !receiver.Comp.Connectable)
                    continue;

                ForceConnectInnerCable(provider, receiver, outerProvider);
            }
        }
    }

    private void OnInnerProviderShutdown(Entity<InnerCableProviderComponent> provider, ref ComponentShutdown args)
    {
        var outerProvider = GetOuterProviderOrNull(provider);
        foreach (var receiver in provider.Comp.ConnectedReceivers.ToList())
        {
            DisconnectInnerCable(provider, receiver, outerProvider);
        }
    }

    private void OnInnerProviderConnected(
        Entity<InnerCableProviderComponent> provider,
        ref ExtensionCableSystem.ProviderConnectedEvent args
    )
    {
        var outerProvider = GetOuterProviderOrNull(provider);
        foreach (var receiver in provider.Comp.ConnectedReceivers)
        {
            ConnectOuterCable(provider, receiver, outerProvider);
        }
    }

    private void OnInnerProviderDisconnected(
        Entity<InnerCableProviderComponent> provider,
        ref ExtensionCableSystem.ProviderDisconnectedEvent args
    )
    {
        var outerProvider = GetOuterProviderOrNull(provider);
        foreach (var receiver in provider.Comp.ConnectedReceivers)
        {
            DisconnectOuterCable(provider, receiver, outerProvider);
        }
    }

    private void OnInsertedIntoInnerProvider(
        Entity<InnerCableProviderComponent> provider,
        ref EntInsertedIntoContainerMessage args
    )
    {
        if (TryComp<InnerCableReceiverComponent>(args.Entity) is not { } receiver ||
            !receiver.Comp.Connectable ||
            !IsContainerConnectable(provider, args.Container))
            return;

        ForceConnectInnerCable(provider, receiver);
    }

    private void OnRemovedFromInnerProvider(
        Entity<InnerCableProviderComponent> provider,
        ref EntRemovedFromContainerMessage args
    )
    {
        if (TryComp<InnerCableReceiverComponent>(args.Entity) is not { } receiver ||
            !provider.Comp.ConnectedReceivers.Contains(receiver))
            return;

        DisconnectInnerCable(provider, receiver);
    }

    private static bool IsContainerConnectable(InnerCableProviderComponent provider, BaseContainer container)
    {
        // Local to sidestep Robust Analyzer access restriction on "executing" a pure function on a readonly field.
        var id = container.ID;
        return !provider.UnconnectableContainers.Contains(id) && provider.ConnectableContainers.Contains(id);
    }

    private Entity<ExtensionCableProviderComponent>? GetOuterProviderOrNull(
        Entity<InnerCableProviderComponent> provider
    )
    {
        return TryComp<ExtensionCableReceiverComponent>(provider)?.Comp.Provider;
    }

    #endregion

    #region Receiver

    private void OnInnerReceiverStarted(Entity<InnerCableReceiverComponent> receiver, ref ComponentStartup args)
    {
        TryConnectToContainingProvider(receiver);
    }

    private void OnInnerReceiverShutdown(Entity<InnerCableReceiverComponent> receiver, ref ComponentShutdown args)
    {
        DisconnectFromContainingProvider(receiver);
    }

    private void TryConnectToContainingProvider(Entity<InnerCableReceiverComponent> receiver)
    {
        if (!receiver.Comp.Connectable ||
            GetContainingInnerProviderOrNull(receiver) is not { } provider ||
            !_container.TryGetContainingContainer(receiver.Owner, out var container) ||
            IsContainerConnectable(provider, container))
            return;

        ForceConnectInnerCable(provider, receiver);
    }

    private void DisconnectFromContainingProvider(Entity<InnerCableReceiverComponent> receiver)
    {
        if (GetContainingInnerProviderOrNull(receiver) is not { } provider ||
            !provider.Comp.ConnectedReceivers.Contains(receiver))
            return;

        DisconnectInnerCable(provider, receiver);
    }

    /// <summary>
    /// Tries to get <paramref name="receiver"/>'s parent as an entity with <see cref="InnerCableProviderComponent"/>.
    /// </summary>
    private Entity<InnerCableProviderComponent>? GetContainingInnerProviderOrNull(
        Entity<InnerCableReceiverComponent> receiver
    )
    {
        return TryComp(receiver, out TransformComponent? xform)
            ? TryComp<InnerCableProviderComponent>(xform.ParentUid)
            : null;
    }

    #endregion

    #region implementation

    /// <summary>
    /// This function assumes checks have been done; all it does is wire up the relationship between the
    /// provider/receiver and dispatch events for relaying the outer connection.
    /// </summary>
    private void ForceConnectInnerCable(
        Entity<InnerCableProviderComponent> provider,
        Entity<InnerCableReceiverComponent> receiver,
        Entity<ExtensionCableProviderComponent>? preResolvedOuterProvider = null
    )
    {
        preResolvedOuterProvider ??= GetOuterProviderOrNull(provider);

        // Disconnect any existing old relationship.
        if (receiver.Comp.Provider is { } oldProvider)
        {
            DisconnectInnerCable(oldProvider, receiver, preResolvedOuterProvider);
        }

        // Connect the Inner Cable relationship.
        provider.Comp.ConnectedReceivers.Add(receiver);
        receiver.Comp.Provider = provider;

        // Connect the "outer" cable relationship via existing events.
        ConnectOuterCable(provider, receiver, preResolvedOuterProvider);
    }

    private void ConnectOuterCable(
        Entity<InnerCableProviderComponent> provider,
        Entity<InnerCableReceiverComponent> receiver,
        Entity<ExtensionCableProviderComponent>? preResolvedOuterProvider = null)
    {
        if ((preResolvedOuterProvider ?? GetOuterProviderOrNull(provider)) is not { } outerProvider ||
            TryComp<ExtensionCableReceiverComponent>(receiver) is not { } innerReceiver)
            return;

        // Connect the "outer" cable relationship via existing events.
        RaiseLocalEvent(
            innerReceiver,
            new ExtensionCableSystem.ProviderConnectedEvent(outerProvider),
            broadcast: false
        );
        RaiseLocalEvent(
            outerProvider,
            new ExtensionCableSystem.ReceiverConnectedEvent(innerReceiver),
            broadcast: false
        );
    }

    private void DisconnectInnerCable(
        Entity<InnerCableProviderComponent> provider,
        Entity<InnerCableReceiverComponent> receiver,
        Entity<ExtensionCableProviderComponent>? preResolvedOuterProvider = null
    )
    {
        // Disconnect the "outer" cable relationship via existing events.
        DisconnectOuterCable(provider, receiver, preResolvedOuterProvider);

        // Disconnect the Inner Cable relationship.
        receiver.Comp.Provider = null;
        provider.Comp.ConnectedReceivers.Remove(receiver);
    }

    private void DisconnectOuterCable(
        Entity<InnerCableProviderComponent> provider,
        Entity<InnerCableReceiverComponent> receiver,
        Entity<ExtensionCableProviderComponent>? preResolvedOuterProvider = null
    )
    {
        // Disconnect the "outer" cable relationship via existing events.
        var outerProvider = preResolvedOuterProvider ?? GetOuterProviderOrNull(provider);

        RaiseLocalEvent(receiver, new ExtensionCableSystem.ProviderDisconnectedEvent(outerProvider), broadcast: false);
        if (outerProvider is { } op &&
            TryComp<ExtensionCableReceiverComponent>(receiver) is { } innerReceiver)
        {
            RaiseLocalEvent(op, new ExtensionCableSystem.ReceiverDisconnectedEvent(innerReceiver), broadcast: false);
        }
    }

    private Entity<T>? TryComp<T>(EntityUid entity) where T : IComponent
    {
        return TryComp(entity, out T? comp) ? new Entity<T>(entity, comp) : null;
    }

    private Entity<T>? ResolveOrNull<T>(Entity<T?> entity) where T : IComponent
    {
        return TryComp<T>(entity.Owner);
    }

    #endregion
}
