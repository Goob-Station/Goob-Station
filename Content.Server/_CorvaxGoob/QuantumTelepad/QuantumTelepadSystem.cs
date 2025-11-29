using Content.Server.Power.Components;
using Content.Shared._CorvaxGoob.QuantumTelepad;
using Content.Shared.DeviceLinking;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server._CorvaxGoob.QuantumTelepad;

public sealed partial class QuantumTelepadSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QuantumTelepadComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<QuantumTelepadComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<QuantumTelepadComponent, ExaminedEvent>(OnExamined);

        // todo: add access configurator compatibility
        // todo: add wires trolling
        // todo: add signal compatibility
    }

    private void OnActivateInWorld(Entity<QuantumTelepadComponent> entity, ref ActivateInWorldEvent args)
    {
        StartTeleport(entity);
    }

    private bool TryGetFirstQuantumConnection(EntityUid entity, [NotNullWhen(true)] out Entity<QuantumTelepadComponent>? connectedEntUid)
    {
        connectedEntUid = null;

        if (!TryComp<DeviceLinkSourceComponent>(entity, out var deviceLinkSink))
            return false;

        foreach (var linkedPort in deviceLinkSink.LinkedPorts)
            foreach (var port in linkedPort.Value)
                if (port.Source == "QuantumTelepad")
                {
                    if (!TryComp<QuantumTelepadComponent>(linkedPort.Key, out var linkedTeleComp))
                        continue;

                    connectedEntUid = (linkedPort.Key, linkedTeleComp);
                    return true;
                }

        return false;
    }

    public void StartTeleport(Entity<QuantumTelepadComponent> entity, bool ignoreDelay = false)
    {
        if (entity.Comp.State is QuantumTelepadState.Teleporting || entity.Comp.State is QuantumTelepadState.ReceiveTeleporting)
            return;

        if (!TryGetFirstQuantumConnection(entity, out var connectedTelepad))
        {
            _popup.PopupEntity(Loc.GetString("quantum-telepad-not-found"), entity);
            return;
        }

        if (!CheckSenderTelepad(entity, connectedTelepad.Value))
            return;

        if (!CheckReceiverTelepad(connectedTelepad.Value))
        {
            _popup.PopupEntity(Loc.GetString("quantum-telepad-receiver-error"), entity);
            return;
        }

        _audio.PlayPvs(entity.Comp.PreTeleportSound, entity);

        entity.Comp.TeleportAt = _timing.CurTime + TimeSpan.FromSeconds(2);
        connectedTelepad.Value.Comp.TeleportAt = _timing.CurTime + TimeSpan.FromSeconds(2);

        SetState(entity, QuantumTelepadState.Teleporting);
        SetState(connectedTelepad.Value, QuantumTelepadState.ReceiveTeleporting);
    }

    private void SetState(Entity<QuantumTelepadComponent> entity, QuantumTelepadState state)
    {
        if (!TryComp<AppearanceComponent>(entity, out var appearance))
            return;

        entity.Comp.State = state;
        _appearance.SetData(entity, QuantumTelepadVisuals.State, state, appearance);
    }

    public void TeleportEntities(Entity<QuantumTelepadComponent> entity, Entity<QuantumTelepadComponent> teleportTo)
    {
        var lookupEntities = _lookup.GetEntitiesInRange(entity, entity.Comp.WorkingRange, flags: entity.Comp.LookupFlag);

        if (lookupEntities.Count == 0)
            return;

        var sendedEnts = 0;

        foreach (var lookupEntity in lookupEntities)
        {
            if (sendedEnts > entity.Comp.MaxEntitiesToTeleportAtOnce)
                break;

            if (entity.Comp.Blacklist is not null && _whitelist.IsBlacklistPass(entity.Comp.Blacklist, lookupEntity))
                continue;
            if (entity.Comp.Whitelist is not null && _whitelist.IsWhitelistFail(entity.Comp.Whitelist, lookupEntity))
                continue;

            _xform.SetWorldPosition(lookupEntity, _xform.GetWorldPosition(teleportTo));

            sendedEnts++;
        }

        if (sendedEnts == 0) // checking for any teleported ents
            return;

        if (entity.Comp.TeleportEffect is not null)
            Spawn(entity.Comp.TeleportEffect, Transform(teleportTo).Coordinates);

        if (entity.Comp.TeleportSound is not null)
        {
            _audio.PlayPvs(entity.Comp.TeleportSound, entity);
            _audio.PlayPvs(entity.Comp.TeleportSound, teleportTo);
        }
    }

    private bool CheckSenderTelepad(Entity<QuantumTelepadComponent> entity, Entity<QuantumTelepadComponent> connectedTelepad, EntityUid? recipient = null, bool ignoreDelay = false)
    {
        if (entity.Comp.MustBeAnchored && !Transform(entity).Anchored)
            return false;

        if (!ignoreDelay && _timing.CurTime < entity.Comp.NextTeleport)
        {
            _popup.PopupEntity(Loc.GetString("quantum-telepad-recharging"), entity);
            return false;
        }

        if (TryComp<ApcPowerReceiverComponent>(entity, out var apcPower) && !apcPower.Powered)
        {
            _popup.PopupClient(Loc.GetString("quantum-telepad-not-powered"), recipient);
            return false;
        }

        if (entity.Comp.CheckConnectionRange && TryComp<DeviceLinkSourceComponent>(entity, out var deviceLinkSink) && !_xform.InRange(Transform(entity).Coordinates, Transform(connectedTelepad).Coordinates, deviceLinkSink.Range))
        {
            _popup.PopupClient(Loc.GetString("quantum-telepad-out-of-range"), recipient);
            return false;
        }

        return true;
    }

    private bool CheckReceiverTelepad(Entity<QuantumTelepadComponent> entity)
    {
        if (entity.Comp.MustBeAnchored && !Transform(entity).Anchored)
            return false;

        if (TryComp<ApcPowerReceiverComponent>(entity, out var apcPower) && !apcPower.Powered)
            return false;

        return true;
    }

    private void OnPowerChanged(Entity<QuantumTelepadComponent> entity, ref PowerChangedEvent args)
    {
        if (args.Powered)
            SetState(entity, QuantumTelepadState.Idle);
        else
            SetState(entity, QuantumTelepadState.Unlit);
    }

    private void OnExamined(Entity<QuantumTelepadComponent> entity, ref ExaminedEvent args)
    {
        if (_timing.CurTime < entity.Comp.NextTeleport)
        {
            args.PushMarkup(Loc.GetString("quantum-telepad-examine-time-remaining",
                ("second", (int)((entity.Comp.NextTeleport - _timing.CurTime).TotalSeconds + 0.5f))));
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<QuantumTelepadComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.State is QuantumTelepadState.Idle || comp.State is QuantumTelepadState.Unlit)
                continue;

            if (_timing.CurTime < comp.TeleportAt)
                continue;

            var curState = comp.State;

            SetState((uid, comp), QuantumTelepadState.Idle);

            if (curState is not QuantumTelepadState.Teleporting)
                continue;

            comp.NextTeleport = _timing.CurTime + TimeSpan.FromSeconds(comp.Delay);

            if (!TryGetFirstQuantumConnection(uid, out var connectedEnt))
            {
                _popup.PopupEntity("quantum-telepad-not-found", uid);
                continue;
            }

            if (!CheckSenderTelepad((uid, comp), connectedEnt.Value, null, true))
                return;

            if (!CheckReceiverTelepad(connectedEnt.Value))
            {
                _popup.PopupEntity("quantum-telepad-receiver-error", uid);
                return;
            }

            TeleportEntities((uid, comp), connectedEnt.Value);
        }
    }
}
