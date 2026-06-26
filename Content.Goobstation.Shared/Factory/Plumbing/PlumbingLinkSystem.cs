using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.Chemistry.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;

namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Handles liquid links for plumbing processors.
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
        if (IsInputLink(ent, args))
        {
            if (!TerminatingOrDeleted(ent.Comp.LinkedInputMachine)
                || !CanProvideSolution(args.Source, args.SourcePort))
            {
                args.Cancel();
            }

            return;
        }

        if (IsOutputLink(ent, args))
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
        if (IsInputLink(ent, args))
        {
            ent.Comp.LinkedInputMachine = args.Source;
            ent.Comp.LinkedInputPort = args.SourcePort;
            ent.Comp.LinkedInputSlot = _automation.GetSlot(args.Source, args.SourcePort, input: false);
            Dirty(ent);
            return;
        }

        if (IsOutputLink(ent, args))
        {
            ent.Comp.LinkedOutputMachine = args.Sink;
            ent.Comp.LinkedOutputPort = args.SinkPort;
            ent.Comp.LinkedOutputSlot = _automation.GetSlot(args.Sink, args.SinkPort, input: true);
            Dirty(ent);
            InvalidatePumpChainsContaining(ent.Owner);
        }
    }

    private void OnPortDisconnected(Entity<PlumbingPortsComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port == ent.Comp.InputId && args.RemovedPortUid == ent.Comp.LinkedInputMachine)
        {
            ent.Comp.LinkedInputMachine = null;
            ent.Comp.LinkedInputPort = null;
            ent.Comp.LinkedInputSlot = null;
            Dirty(ent);
        }

        if (args.Port == ent.Comp.OutputId && args.RemovedPortUid == ent.Comp.LinkedOutputMachine)
        {
            ent.Comp.LinkedOutputMachine = null;
            ent.Comp.LinkedOutputPort = null;
            ent.Comp.LinkedOutputSlot = null;
            Dirty(ent);
            InvalidatePumpChainsContaining(ent.Owner);
        }
    }

    private bool IsInputLink(Entity<PlumbingPortsComponent> ent, LinkAttemptEvent args)
    {
        return args.Sink == ent.Owner && args.SinkPort == ent.Comp.InputId;
    }

    private bool IsInputLink(Entity<PlumbingPortsComponent> ent, NewLinkEvent args)
    {
        return args.Sink == ent.Owner && args.SinkPort == ent.Comp.InputId;
    }

    private bool IsOutputLink(Entity<PlumbingPortsComponent> ent, LinkAttemptEvent args)
    {
        return args.Source == ent.Owner && args.SourcePort == ent.Comp.OutputId;
    }

    private bool IsOutputLink(Entity<PlumbingPortsComponent> ent, NewLinkEvent args)
    {
        return args.Source == ent.Owner && args.SourcePort == ent.Comp.OutputId;
    }

    private bool CanProvideSolution(EntityUid uid, string port)
    {
        return _automation.HasSlot(uid, port, input: false)
            || TryComp<PlumbingPortsComponent>(uid, out var plumbing) && plumbing.OutputId == port;
    }

    private bool CanReceiveSolution(EntityUid uid, string port)
    {
        return _automation.HasSlot(uid, port, input: true)
            || TryComp<PlumbingPortsComponent>(uid, out var plumbing) && plumbing.InputId == port;
    }

    public bool TryGetInput(EntityUid uid, out EntityUid machine, out string port)
    {
        if (TryComp<PlumbingPortsComponent>(uid, out var comp)
            && comp.LinkedInputMachine is { } linkedMachine
            && comp.LinkedInputPort is { } linkedPort)
        {
            machine = linkedMachine;
            port = linkedPort;
            return true;
        }

        machine = default;
        port = string.Empty;
        return false;
    }

    public bool TryGetOutput(EntityUid uid, out EntityUid machine, out string port)
    {
        if (TryComp<PlumbingPortsComponent>(uid, out var comp)
            && comp.LinkedOutputMachine is { } linkedMachine
            && comp.LinkedOutputPort is { } linkedPort)
        {
            machine = linkedMachine;
            port = linkedPort;
            return true;
        }

        machine = default;
        port = string.Empty;
        return false;
    }

    private static bool TryGetSolution(AutomationSlot? slot, out Entity<SolutionComponent> solution)
    {
        solution = default;

        if (slot?.GetSolution() is not { } slotSolution)
            return false;

        solution = slotSolution;
        return true;
    }

    /// <summary>
    /// Resolves the real upstream solution feeding a plumbing processor.
    /// </summary>
    /// <param name="ent">The plumbing processor to resolve from.</param>
    /// <param name="input">The upstream solution entity.</param>
    /// <returns>True if the processor's input link resolves to a solution.</returns>
    public bool TryGetInputSolution(Entity<PlumbingPumpComponent> ent, out Entity<SolutionComponent> input)
    {
        input = default;

        if (!TryComp<PlumbingPortsComponent>(ent.Owner, out var ports)
            || ports.LinkedInputMachine == null
            || ports.LinkedInputPort == null)
            return false;

        return TryGetSolution(ports.LinkedInputSlot, out input);
    }

    /// <summary>
    /// Follows downstream plumbing processors until a real output solution is found.
    /// </summary>
    public bool TryResolveOutputChain(Entity<PlumbingPumpComponent> ent, out List<EntityUid> processors, out Entity<SolutionComponent> output)
    {
        if (!ent.Comp.ChainDirty)
        {
            if (TryResolveCachedOutputChain(ent, out processors, out output))
                return true;

            if (!ent.Comp.ChainDirty)
                return false;
        }

        return RebuildOutputChain(ent, out processors, out output);
    }

    private bool TryResolveCachedOutputChain(Entity<PlumbingPumpComponent> ent, out List<EntityUid> processors, out Entity<SolutionComponent> output)
    {
        processors = ent.Comp.CachedProcessors;
        output = default;

        foreach (var processor in processors)
        {
            if (!HasComp<PlumbingProcessorComponent>(processor))
            {
                ent.Comp.ChainDirty = true;
                return false;
            }
        }

        return ent.Comp.CachedOutputMachine is not null
               && ent.Comp.CachedOutputPort is not null
               && TryGetSolution(ent.Comp.CachedOutputSlot, out output);
    }

    private bool RebuildOutputChain(Entity<PlumbingPumpComponent> ent, out List<EntityUid> processors, out Entity<SolutionComponent> output)
    {
        processors = ent.Comp.CachedProcessors;
        processors.Clear();

        ent.Comp.CachedOutputMachine = null;
        ent.Comp.CachedOutputPort = null;
        ent.Comp.CachedOutputSlot = null;
        output = default;

        var current = ent.Owner;

        while (true)
        {
            if (processors.Contains(current))
            {
                ent.Comp.ChainDirty = false;
                return false;
            }

            if (!HasComp<PlumbingProcessorComponent>(current))
            {
                ent.Comp.ChainDirty = false;
                return false;
            }

            processors.Add(current);

            if (!TryGetOutput(current, out var outputMachine, out var outputPort))
            {
                ent.Comp.ChainDirty = false;
                return false;
            }

            var outputSlot = TryComp<PlumbingPortsComponent>(current, out var ports)
                ? ports.LinkedOutputSlot
                : null;

            if (TryGetSolution(outputSlot, out output))
            {
                ent.Comp.CachedOutputMachine = outputMachine;
                ent.Comp.CachedOutputPort = outputPort;
                ent.Comp.CachedOutputSlot = outputSlot;
                ent.Comp.ChainDirty = false;
                return true;
            }

            if (!HasComp<PlumbingProcessorComponent>(outputMachine))
            {
                ent.Comp.CachedOutputMachine = outputMachine;
                ent.Comp.CachedOutputPort = outputPort;
                ent.Comp.ChainDirty = false;
                return false;
            }

            current = outputMachine;
        }
    }

    private void InvalidatePumpChainsContaining(EntityUid processor)
    {
        var query = EntityQueryEnumerator<PlumbingPumpComponent>();

        while (query.MoveNext(out var uid, out var pump))
        {
            if (uid == processor || pump.CachedProcessors.Contains(processor))
                pump.ChainDirty = true;
        }
    }
}
