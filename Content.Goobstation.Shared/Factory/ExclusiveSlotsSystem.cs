// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;

namespace Content.Goobstation.Shared.Factory;

/// <summary>
/// Tracks port/machine state and prevents linking multiple machines to exclusive ports.
/// </summary>
public sealed class ExclusiveSlotsSystem : EntitySystem
{
    [Dependency] private readonly AutomationSystem _automation = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _device = default!;

    private EntityQuery<ExclusiveInputSlotComponent> _inputQuery;
    private EntityQuery<ExclusiveOutputSlotComponent> _outputQuery;

    public override void Initialize()
    {
        base.Initialize();

        _inputQuery = GetEntityQuery<ExclusiveInputSlotComponent>();
        _outputQuery = GetEntityQuery<ExclusiveOutputSlotComponent>();

        SubscribeLocalEvent<ExclusiveInputSlotComponent, ComponentInit>(OnInputInit);
        SubscribeLocalEvent<ExclusiveInputSlotComponent, LinkAttemptEvent>(OnInputLinkAttempt);
        SubscribeLocalEvent<ExclusiveInputSlotComponent, NewLinkEvent>(OnInputNewLink);
        SubscribeLocalEvent<ExclusiveInputSlotComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<ExclusiveOutputSlotComponent, ComponentInit>(OnOutputInit);
        SubscribeLocalEvent<ExclusiveOutputSlotComponent, LinkAttemptEvent>(OnOutputLinkAttempt);
        SubscribeLocalEvent<ExclusiveOutputSlotComponent, NewLinkEvent>(OnOutputNewLink);
        SubscribeLocalEvent<ExclusiveOutputSlotComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    #region Non-generic glue
    private void OnInputInit(Entity<ExclusiveInputSlotComponent> ent, ref ComponentInit args)
    {
        _device.EnsureSinkPorts(ent, ent.Comp.Port);
        UpdateSlot(ent.Comp);
    }

    private void OnInputLinkAttempt(Entity<ExclusiveInputSlotComponent> ent, ref LinkAttemptEvent args)
    {
        if (TryCancelLink((ent, ent.Comp), args.Sink, args.SinkPort, args.Source, args.SourcePort))
            args.Cancel();
    }

    private void OnInputNewLink(Entity<ExclusiveInputSlotComponent> ent, ref NewLinkEvent args)
    {
        NewLink(ent, args.Sink, args.SinkPort, args.Source, args.SourcePort);
    }

    private void OnOutputInit(Entity<ExclusiveOutputSlotComponent> ent, ref ComponentInit args)
    {
        _device.EnsureSourcePorts(ent, ent.Comp.Port);
        UpdateSlot(ent.Comp);
    }

    private void OnOutputLinkAttempt(Entity<ExclusiveOutputSlotComponent> ent, ref LinkAttemptEvent args)
    {
        if (TryCancelLink((ent, ent.Comp), args.Source, args.SourcePort, args.Sink, args.SinkPort))
            args.Cancel();
    }

    private void OnOutputNewLink(Entity<ExclusiveOutputSlotComponent> ent, ref NewLinkEvent args)
    {
        NewLink(ent, args.Source, args.SourcePort, args.Sink, args.SinkPort);
    }
    #endregion

    #region Generic helpers
    private bool TryCancelLink(Entity<IExclusiveSlotComponent> ent, EntityUid sink, string sinkPort, EntityUid source, string sourcePort)
    {
        if (sink != ent.Owner || sinkPort != ent.Comp.PortId)
            return false;

        // only 1 machine can be linked to the port
        return !TerminatingOrDeleted(ent.Comp.LinkedMachine) ||
            // prevent linking to random non-automation slot
            !_automation.HasSlot(source, sourcePort, input: !ent.Comp.IsInput);
    }

    private void NewLink<T>(Entity<T> ent, EntityUid other, string otherPort, EntityUid uid, string port)
        where T: IExclusiveSlotComponent
    {
        if (other != ent.Owner || otherPort != ent.Comp.PortId)
            return;

        ent.Comp.LinkedMachine = uid;
        ent.Comp.LinkedPort = port;
        ent.Comp.LinkedSlot = _automation.GetSlot(uid, port, input: !ent.Comp.IsInput);
        Dirty(ent);
    }

    private void OnPortDisconnected<T>(Entity<T> ent, ref PortDisconnectedEvent args)
        where T: IExclusiveSlotComponent
    {
        // this event is shit and doesnt have source/sink entity and port just 1 string
        // so if you made InputPort and OutputPort the same string it would silently break
        // absolute supercode
        if (args.Port != ent.Comp.PortId)
            return;

        ent.Comp.LinkedMachine = null;
        ent.Comp.LinkedPort = null;
        ent.Comp.LinkedSlot = null;
        Dirty(ent);
    }

    private void OnHandleState<T>(Entity<T> ent, ref AfterAutoHandleStateEvent args)
        where T: IExclusiveSlotComponent
    {
        // incase client didnt predict linked port changing, update them
        UpdateSlot(ent.Comp);
    }

    public void UpdateSlot(IExclusiveSlotComponent comp)
    {
        if (comp.LinkedMachine is {} machine && comp.LinkedPort is {} port)
            comp.LinkedSlot = _automation.GetSlot(machine, port, input: comp.IsInput);
    }
    #endregion

    #region Public API
    /// <summary>
    /// Get the linked input machine and slot for this machine.
    /// Returns true if machine is non-default and slot is non-null.
    /// </summary>
    public bool GetInput(EntityUid uid, out EntityUid machine, out AutomationSlot slot)
    {
        if (_inputQuery.TryComp(uid, out var comp) && comp.LinkedMachine is {} m && comp.LinkedSlot is {} s)
        {
            machine = m;
            slot = s;
            return true;
        }

        machine = default!;
        slot = default!;
        return false;
    }

    /// <summary>
    /// Get the linked input slot only for this machine.
    /// </summary>
    public AutomationSlot? GetInputSlot(EntityUid uid)
        => _inputQuery.CompOrNull(uid)?.LinkedSlot;

    /// <summary>
    /// Get the linked output machine and slot for this machine.
    /// Returns true if machine is non-default and slot is non-null.
    /// </summary>
    public bool GetOutput(EntityUid uid, out EntityUid machine, out AutomationSlot slot)
    {
        if (_outputQuery.TryComp(uid, out var comp) && comp.LinkedMachine is {} m && comp.LinkedSlot is {} s)
        {
            machine = m;
            slot = s;
            return true;
        }

        machine = default!;
        slot = default!;
        return false;
    }

    /// <summary>
    /// Get the linked output slot only for this machine.
    /// </summary>
    public AutomationSlot? GetOutputSlot(EntityUid uid)
        => _outputQuery.CompOrNull(uid)?.LinkedSlot;
    #endregion
}
