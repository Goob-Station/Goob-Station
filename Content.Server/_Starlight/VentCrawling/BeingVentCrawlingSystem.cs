using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Shared._Starlight.VentCrawling.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;

namespace Content.Server._Starlight.VentCrawling;

public sealed class BeingVentCrawSystem : EntitySystem
{
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeingVentCrawlerComponent, InhaleLocationEvent>(OnInhaleLocation);
        SubscribeLocalEvent<BeingVentCrawlerComponent, ExhaleLocationEvent>(OnExhaleLocation);
        SubscribeLocalEvent<BeingVentCrawlerComponent, AtmosExposedGetAirEvent>(OnGetAir);

        SubscribeLocalEvent<BeingVentCrawlerComponent, ActionAttemptEvent>(OnActionAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, AttemptMeleeEvent>(OnMeleeAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, ShotAttemptedEvent>(OnShootAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, IsUnequippingAttemptEvent>(OnUnequiptAttempt);
        SubscribeLocalEvent<BeingVentCrawlerComponent, IsEquippingAttemptEvent>(OnEquiptAttempt);

    }

    private void OnGetAir(EntityUid uid, BeingVentCrawlerComponent component, ref AtmosExposedGetAirEvent args)
    {
        if (!TryComp<VentCrawlerHolderComponent>(component.Holder, out var holder))
            return;

        if (holder.CurrentTube == null)
            return;

        if (!TryComp(holder.CurrentTube.Value, out NodeContainerComponent? nodeContainer))
            return;

        foreach (var nodeContainerNode in nodeContainer.Nodes)
        {
            if (!_nodeContainer.TryGetNode(nodeContainer, nodeContainerNode.Key, out PipeNode? pipe))
                continue;
            args.Gas = pipe.Air;
            args.Handled = true;
            return;
        }
    }

    private void OnInhaleLocation(EntityUid uid, BeingVentCrawlerComponent component, InhaleLocationEvent args)
    {
        if (!TryComp<VentCrawlerHolderComponent>(component.Holder, out var holder))
            return;

        if (holder.CurrentTube == null)
            return;

        if (!TryComp(holder.CurrentTube.Value, out NodeContainerComponent? nodeContainer))
            return;

        foreach (var nodeContainerNode in nodeContainer.Nodes)
        {
            if (!_nodeContainer.TryGetNode(nodeContainer, nodeContainerNode.Key, out PipeNode? pipe))
                continue;
            args.Gas = pipe.Air;
            return;
        }
    }

    private void OnExhaleLocation(EntityUid uid, BeingVentCrawlerComponent component, ExhaleLocationEvent args)
    {
        if (!TryComp<VentCrawlerHolderComponent>(component.Holder, out var holder))
            return;

        if (holder.CurrentTube == null)
            return;

        if (!TryComp(holder.CurrentTube.Value, out NodeContainerComponent? nodeContainer))
            return;

        foreach (var nodeContainerNode in nodeContainer.Nodes)
        {
            if (!_nodeContainer.TryGetNode(nodeContainer, nodeContainerNode.Key, out PipeNode? pipe))
                continue;
            args.Gas = pipe.Air;
            return;
        }
    }

    private void OnActionAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref ActionAttemptEvent args)
        => args.Cancelled = true;

    private void OnMeleeAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref AttemptMeleeEvent args)
        => args.Cancelled = true;

    private void OnShootAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref ShotAttemptedEvent args)
        => args.Cancel();

    private void OnDropAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref DropAttemptEvent args)
        => args.Cancel();

    private void OnUnequiptAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref IsUnequippingAttemptEvent args)
        => args.Cancel();

    private void OnEquiptAttempt(EntityUid uid, BeingVentCrawlerComponent component, ref IsEquippingAttemptEvent args)
        => args.Cancel();
}
