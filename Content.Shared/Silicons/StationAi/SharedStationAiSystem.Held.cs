// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ScarKy0 <scarky0@onet.eu>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions.Events;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.StationAi;

public abstract partial class SharedStationAiSystem
{
    /*
     * Added when an entity is inserted into a StationAiCore.
     */

    //TODO: Fix this, please
    private const string JobNameLocId = "job-name-station-ai";

    private void InitializeHeld()
    {
        SubscribeLocalEvent<StationAiRadialMessage>(OnRadialMessage);
        SubscribeLocalEvent<BoundUserInterfaceMessageAttempt>(OnMessageAttempt);
        SubscribeLocalEvent<StationAiWhitelistComponent, GetVerbsEvent<AlternativeVerb>>(OnTargetVerbs);

        SubscribeLocalEvent<StationAiHeldComponent, InteractionAttemptEvent>(OnHeldInteraction);
        SubscribeLocalEvent<StationAiHeldComponent, AttemptRelayActionComponentChangeEvent>(OnHeldRelay);
        SubscribeLocalEvent<StationAiHeldComponent, JumpToCoreEvent>(OnCoreJump);
        SubscribeLocalEvent<TryGetIdentityShortInfoEvent>(OnTryGetIdentityShortInfo);
    }

    private void OnTryGetIdentityShortInfo(TryGetIdentityShortInfoEvent args)
    {
        if (args.Handled)
        {
            return;
        }

        if (!HasComp<StationAiHeldComponent>(args.ForActor))
        {
            return;
        }
        args.Title = $"{Name(args.ForActor)} ({Loc.GetString(JobNameLocId)})";
        args.Handled = true;
    }

    private void OnCoreJump(Entity<StationAiHeldComponent> ent, ref JumpToCoreEvent args)
    {
        if (!TryGetCore(ent.Owner, out var core) || core.Comp?.RemoteEntity == null)
            return;

        _xforms.DropNextTo(core.Comp.RemoteEntity.Value, core.Owner) ;
    }

    /// <summary>
    /// Tries to get the entity held in the AI core using StationAiCore.
    /// </summary>
    public bool TryGetHeld(Entity<StationAiCoreComponent?> entity, out EntityUid held)
    {
        held = EntityUid.Invalid;

        if (!Resolve(entity.Owner, ref entity.Comp))
            return false;

        if (!_containers.TryGetContainer(entity.Owner, StationAiCoreComponent.Container, out var container) ||
            container.ContainedEntities.Count == 0)
            return false;

        held = container.ContainedEntities[0];
        return true;
    }

    /// <summary>
    /// Tries to get the entity held in the AI using StationAiHolder.
    /// </summary>
    public bool TryGetHeld(Entity<StationAiHolderComponent?> entity, out EntityUid held)
    {
        TryComp<StationAiCoreComponent>(entity.Owner, out var stationAiCore);

        return TryGetHeld((entity.Owner, stationAiCore), out held);
    }

    public bool TryGetCore(EntityUid entity, out Entity<StationAiCoreComponent?> core)
    {
        var xform = Transform(entity);
        var meta = MetaData(entity);
        var ent = new Entity<TransformComponent?, MetaDataComponent?>(entity, xform, meta);

        if (!_containers.TryGetContainingContainer(ent, out var container) ||
            container.ID != StationAiCoreComponent.Container ||
            !TryComp(container.Owner, out StationAiCoreComponent? coreComp) ||
            coreComp.RemoteEntity == null)
        {
            core = (EntityUid.Invalid, null);
            return false;
        }

        core = (container.Owner, coreComp);
        return true;
    }

    private void OnHeldRelay(Entity<StationAiHeldComponent> ent, ref AttemptRelayActionComponentChangeEvent args)
    {
        if (!TryGetCore(ent.Owner, out var core))
            return;

        args.Target = core.Comp?.RemoteEntity;
    }

    private void OnRadialMessage(StationAiRadialMessage ev)
    {
        if (!TryGetEntity(ev.Entity, out var target))
            return;

        ev.Event.User = ev.Actor;
        RaiseLocalEvent(target.Value, (object) ev.Event);
    }

    private void OnMessageAttempt(BoundUserInterfaceMessageAttempt ev)
    {
        if (ev.Actor == ev.Target)
            return;

        if (TryComp(ev.Actor, out StationAiHeldComponent? aiComp) &&
           (!TryComp(ev.Target, out StationAiWhitelistComponent? whitelistComponent) ||
            !ValidateAi((ev.Actor, aiComp))))
        {
            if (whitelistComponent is { Enabled: false })
            {
                ShowDeviceNotRespondingPopup(ev.Actor);
            }
            ev.Cancel();
        }
    }

    private void OnHeldInteraction(Entity<StationAiHeldComponent> ent, ref InteractionAttemptEvent args)
    {
        // Cancel if it's not us or something with a whitelist, or whitelist is disabled.
        args.Cancelled = (!TryComp(args.Target, out StationAiWhitelistComponent? whitelistComponent)
                          || !whitelistComponent.Enabled)
                         && ent.Owner != args.Target
                         && args.Target != null;
        if (whitelistComponent is { Enabled: false })
        {
            ShowDeviceNotRespondingPopup(ent.Owner);
        }
    }

    private void OnTargetVerbs(Entity<StationAiWhitelistComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanComplexInteract
            || !HasComp<StationAiHeldComponent>(args.User))
        {
            return;
        }

        var user = args.User;

        var target = args.Target;

        var isOpen = _uiSystem.IsUiOpen(target, AiUi.Key, user);

        var verb = new AlternativeVerb
        {
            Text = isOpen ? Loc.GetString("ai-close") : Loc.GetString("ai-open"),
            Act = () =>
            {
                // no need to show menu if device is not powered.
                if (!PowerReceiver.IsPowered(ent.Owner))
                {
                    ShowDeviceNotRespondingPopup(user);
                    return;
                }

                if (isOpen)
                {
                    _uiSystem.CloseUi(ent.Owner, AiUi.Key, user);
                }
                else
                {
                    _uiSystem.OpenUi(ent.Owner, AiUi.Key, user);
                }
            }
        };
        args.Verbs.Add(verb);
    }

    private void ShowDeviceNotRespondingPopup(EntityUid toEntity)
    {
        _popup.PopupClient(Loc.GetString("ai-device-not-responding"), toEntity, PopupType.MediumCaution);
    }
}

/// <summary>
/// Raised from client to server as a BUI message wrapping the event to perform.
/// Also handles AI action validation.
/// </summary>
[Serializable, NetSerializable]
public sealed class StationAiRadialMessage : BoundUserInterfaceMessage
{
    public BaseStationAiAction Event = default!;
}

// Do nothing on server just here for shared move along.
/// <summary>
/// Raised on client to get the relevant data for radial actions.
/// </summary>
public sealed class StationAiRadial : BaseStationAiAction
{
    public SpriteSpecifier? Sprite;

    public string? Tooltip;

    public BaseStationAiAction Event = default!;
}

/// <summary>
/// Abstract parent for radial actions events.
/// When a client requests a radial action this will get sent.
/// </summary>
[Serializable, NetSerializable]
public abstract class BaseStationAiAction
{
    [field:NonSerialized]
    public EntityUid User { get; set; }
}

// No idea if there's a better way to do this.
/// <summary>
/// Grab actions possible for an AI on the target entity.
/// </summary>
[ByRefEvent]
public record struct GetStationAiRadialEvent()
{
    public List<StationAiRadial> Actions = new();
}

[Serializable, NetSerializable]
public enum AiUi : byte
{
    Key,
}