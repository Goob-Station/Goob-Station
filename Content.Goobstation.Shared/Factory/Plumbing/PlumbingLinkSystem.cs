using Content.Shared.Chemistry.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;

namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Handles liquid links for chained pumps
/// </summary>
public sealed class PlumbingLinkSystem : EntitySystem
{
    [Dependency] private readonly AutomationSystem _automation = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _device = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlumbingPortsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PlumbingPortsComponent, LinkAttemptEvent>(OnLinkAttempt);
        SubscribeLocalEvent<PlumbingPortsComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<PlumbingPortsComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    private void OnInit(Entity<PlumbingPortsComponent> ent, ref ComponentInit args)
    {
        _device.EnsureSinkPorts(ent, ent.Comp.Input);
        _device.EnsureSourcePorts(ent, ent.Comp.Output);
    }

    private void OnLinkAttempt(Entity<PlumbingPortsComponent> ent, ref LinkAttemptEvent args)
    {
        if (args.Source == ent.Owner
            && args.SourcePort == ent.Comp.Output.Id
            && args.Sink == ent.Owner
            && args.SinkPort == ent.Comp.Input.Id)
        {
            args.Cancel();
            return;
        }

        if (args.Sink == ent.Owner && args.SinkPort == ent.Comp.Input.Id)
        {
            if (!TerminatingOrDeleted(ent.Comp.LinkedInputMachine)
                || !CanProvideSolution(args.Source, args.SourcePort))
            {
                args.Cancel();
            }

            return;
        }

        if (args.Source == ent.Owner && args.SourcePort == ent.Comp.Output.Id)
        {
            if (!TerminatingOrDeleted(ent.Comp.LinkedOutputMachine)
                || !CanReceiveSolution(args.Sink, args.SinkPort))
            {
                args.Cancel();
            }
        }
    }

    private void OnNewLink(Entity<PlumbingPortsComponent> ent, ref NewLinkEvent args)
    {
        if (args.Sink == ent.Owner && args.SinkPort == ent.Comp.Input.Id)
        {
            ent.Comp.LinkedInputMachine = args.Source;
            ent.Comp.LinkedInputPort = args.SourcePort;
            Dirty(ent);
            return;
        }

        if (args.Source == ent.Owner && args.SourcePort == ent.Comp.Output.Id)
        {
            ent.Comp.LinkedOutputMachine = args.Sink;
            ent.Comp.LinkedOutputPort = args.SinkPort;
            Dirty(ent);
        }
    }

    private void OnPortDisconnected(Entity<PlumbingPortsComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port == ent.Comp.Input.Id && args.RemovedPortUid == ent.Comp.LinkedInputMachine)
        {
            ent.Comp.LinkedInputMachine = null;
            ent.Comp.LinkedInputPort = null;
            Dirty(ent);
        }

        if (args.Port == ent.Comp.Output.Id && args.RemovedPortUid == ent.Comp.LinkedOutputMachine)
        {
            ent.Comp.LinkedOutputMachine = null;
            ent.Comp.LinkedOutputPort = null;
            Dirty(ent);
        }
    }

    private bool CanProvideSolution(EntityUid uid, string port)
    {
        return _automation.HasSlot(uid, port, input: false)
            || TryComp<PlumbingPortsComponent>(uid, out var plumbing) && plumbing.Output.Id == port;
    }

    private bool CanReceiveSolution(EntityUid uid, string port)
    {
        return _automation.HasSlot(uid, port, input: true)
            || TryComp<PlumbingPortsComponent>(uid, out var plumbing) && plumbing.Input.Id == port;
    }

    /// <summary>
    /// Resolves the source solution linked to a pump.
    /// </summary>
    /// <param name="uid">The pump  to resolve from.</param>
    /// <param name="input">The upstream solution entity.</param>
    /// <returns>True if the pump's input link resolves to a solution.</returns>
    public bool TryGetInputSolution(EntityUid uid, out Entity<SolutionComponent> input)
    {
        input = default;

        if (!TryComp<PlumbingPortsComponent>(uid, out var ports)
            || ports.LinkedInputMachine is not { } inputMachine
            || ports.LinkedInputPort is not { } inputPort
            || _automation.GetSlot(inputMachine, inputPort, input: false)?.GetSolution() is not { } solution)
            return false;

        input = solution;
        return true;
    }

    /// <summary>
    /// Follows downstream pump s until a real output solution is found.
    /// </summary>
    public bool TryResolveOutputChain(EntityUid uid, out List<EntityUid> pumps, out Entity<SolutionComponent> output)
    {
        pumps = new();
        output = default;

        var current = uid;

        while (true)
        {
            if (pumps.Contains(current))
            {
                return false;
            }

            if (!TryComp<PlumbingPortsComponent>(current, out var ports)
                || ports.LinkedOutputMachine is not { } outputMachine
                || ports.LinkedOutputPort is not { } outputPort)
            {
                return false;
            }

            pumps.Add(current);

            if (_automation.GetSlot(outputMachine, outputPort, input: true)?.GetSolution() is { } solution)
            {
                output = solution;
                return true;
            }

            current = outputMachine;
        }
    }
}
