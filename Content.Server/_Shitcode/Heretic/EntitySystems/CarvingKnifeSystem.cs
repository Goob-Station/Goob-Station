// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Heretic;
using Content.Server.Chat.Managers;
using Content.Server.DoAfter;
using Content.Server.Gravity;
using Content.Server.Mind;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Heretic.EntitySystems;

public sealed class CarvingKnifeSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly Shared.StatusEffectNew.StatusEffectsSystem _statusNew = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private static readonly ProtoId<TagPrototype> CarvingTag = "HereticCarving";
    private static readonly EntProtoId AlertEffect = "CarvingAlertedStatusEffect";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CarvingKnifeComponent, RuneCarvingSelectedMessage>(OnCarvingSelected);
        SubscribeLocalEvent<CarvingKnifeComponent, CarveRuneDoAfterEvent>(OnCarveRune);
        SubscribeLocalEvent<CarvingKnifeComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CarvingKnifeComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<CarvingKnifeComponent, DeleteAllCarvingsEvent>(OnDeleteCarvings);
        SubscribeLocalEvent<CarvingKnifeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CarvingKnifeComponent, GetItemActionsEvent>(OnGetItemActions);

        SubscribeLocalEvent<AlertCarvingComponent, RuneCarvedEvent>(OnRuneCarved);
        SubscribeLocalEvent<AlertCarvingComponent, TrapTriggeredEvent>(OnAlertTriggered);
        SubscribeLocalEvent<MadCarvingComponent, TrapTriggeredEvent>(OnMadTriggered);

        SubscribeNetworkEvent<ButtonTagPressedEvent>(OnPressed);
    }

    private void OnPressed(ButtonTagPressedEvent ev)
    {
        if (ev.Id != CarvingAlertedStatusEffectComponent.Id)
            return;

        if (!TryGetEntity(ev.User, out var ent))
            return;

        if (!_statusNew.TryEffectsWithComp<CarvingAlertedStatusEffectComponent>(ent.Value, out var effects) ||
            effects.Count == 0)
            return;

        var effect = effects.First();

        if (!effect.Comp1.Locations.Contains(ev.Coords))
            return;

        var coords = GetCoordinates(ev.Coords);
        _pulling.StopAllPulls(ent.Value);
        _transform.SetCoordinates(ent.Value, coords);
        _audio.PlayPvs(effect.Comp1.TeleportSound, coords);
        _statusNew.TryRemoveStatusEffect(ent.Value, AlertEffect);
    }

    private void OnGetItemActions(Entity<CarvingKnifeComponent> ent, ref GetItemActionsEvent args)
    {
        if (!args.InHands)
            return;

        if (!HasComp<HereticComponent>(args.User) && !HasComp<GhoulComponent>(args.User))
            return;

        args.AddAction(ent.Comp.RunebreakActionEntity);
    }

    private void OnMapInit(Entity<CarvingKnifeComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.RunebreakActionEntity, ent.Comp.RunebreakAction);
    }

    private void OnMadTriggered(Entity<MadCarvingComponent> ent, ref TrapTriggeredEvent args)
    {
        _stamina.TakeStaminaDamage(args.Victim, ent.Comp.StaminaDamage);

        if (!TryComp(args.Victim, out StatusEffectsComponent? status))
            return;

        _status.TryAddStatusEffect<TemporaryBlindnessComponent>(args.Victim,
            "TemporaryBlindness",
            ent.Comp.BlindnessTime,
            true,
            status);

        _status.TryAddStatusEffect<MutedComponent>(args.Victim,
            "Muted",
            ent.Comp.MuteTime,
            true,
            status);
    }

    private void OnAlertTriggered(Entity<AlertCarvingComponent> ent, ref TrapTriggeredEvent args)
    {
        if (args.Victim == ent.Comp.User)
            return;

        if (!TryComp(ent.Comp.User, out ActorComponent? actor))
            return;

        var netUser = GetNetEntity(ent.Comp.User.Value);
        var coords = GetNetCoordinates(Transform(ent).Coordinates);
        var coordsLoc = Loc.GetString("alert-carving-trigger-message-coords",
            ("uid", coords.NetEntity.Id),
            ("x", coords.X),
            ("y", coords.Y));

        var location = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(ent.Owner));
        var message = Loc.GetString("alert-carving-trigger-message",
            ("victim", args.Victim),
            ("location", location),
            ("timer", ent.Comp.TeleportDelay),
            ("id", CarvingAlertedStatusEffectComponent.Id),
            ("uid", netUser.Id),
            ("coords", coordsLoc));
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToOne(ChatChannel.Server,
            message,
            wrappedMessage,
            default,
            false,
            actor.PlayerSession.Channel,
            Color.DarkGreen,
            canCoalesce: false);
        _audio.PlayGlobal(ent.Comp.AlertSound, actor.PlayerSession);
        if (_statusNew.TryUpdateStatusEffectDuration(ent.Comp.User.Value,
                AlertEffect,
                out var effect,
                TimeSpan.FromMilliseconds(ent.Comp.TeleportDelay + 100)))
            EnsureComp<CarvingAlertedStatusEffectComponent>(effect.Value).Locations.Add(coords);
    }

    private void OnDeleteCarvings(Entity<CarvingKnifeComponent> ent, ref DeleteAllCarvingsEvent args)
    {
        args.Handled = true;

        _popup.PopupEntity(Loc.GetString("carving-knife-comp-runes-deleted"), args.Performer, args.Performer);

        foreach (var rune in ent.Comp.DrawnRunes.Where(Exists))
        {
            QueueDel(rune);
        }

        ent.Comp.DrawnRunes.Clear();
    }

    private void OnRuneCarved(Entity<AlertCarvingComponent> ent, ref RuneCarvedEvent args)
    {
        ent.Comp.User = args.User;
    }

    private void OnExamine(Entity<CarvingKnifeComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<HereticComponent>(args.Examiner) && !HasComp<GhoulComponent>(args.Examiner))
            return;

        UpdateRunes(ent);

        var loc = Loc.GetString("carving-knife-comp-runes-count", ("count", ent.Comp.DrawnRunes.Count));
        args.PushMarkup(loc);
    }

    private void OnAfterInteract(Entity<CarvingKnifeComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !_tag.HasTag(args.Target.Value, CarvingTag))
            return;

        if (!HasComp<HereticComponent>(args.User) && !HasComp<GhoulComponent>(args.User))
            return;

        QueueDel(args.Target.Value);
        _audio.PlayPvs(ent.Comp.Sound, ent);

        args.Handled = true;
    }

    private void OnCarveRune(Entity<CarvingKnifeComponent> ent, ref CarveRuneDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var coords = _transform.GetMapCoordinates(args.User);

        if (!CanDrawRune(args.User, coords))
            return;

        if (CheckOtherCarvingsNearby(coords))
            return;

        if (!_prototype.TryIndex(args.Carving, out var index))
            return;

        args.Handled = true;

        var rune = Spawn(index.ProtoId, coords);
        var ev = new RuneCarvedEvent(args.User);
        RaiseLocalEvent(rune, ref ev);
        ent.Comp.DrawnRunes.Add(rune);

        if (!TryComp(rune, out WizardTrapComponent? trap) || !_mind.TryGetMind(args.User, out var mind, out _))
            return;

        trap.IgnoredMinds.Add(mind);
        Dirty(rune, trap);
    }

    private void UpdateRunes(Entity<CarvingKnifeComponent> ent)
    {
        ent.Comp.DrawnRunes.RemoveAll(x => !Exists(x));
    }

    private bool CanDrawRune(EntityUid user, MapCoordinates mapCoords)
    {
        if (!_mapMan.TryFindGridAt(mapCoords, out var gridUid, out var gridComp))
            return !_gravity.IsWeightless(user);

        if (!_map.TryGetTileDef(gridComp, _map.TileIndicesFor(gridUid, gridComp, mapCoords), out var tile))
            return false;

        return ((ContentTileDefinition) tile).ID != ContentTileDefinition.SpaceID;
    }

    private bool CheckOtherCarvingsNearby(MapCoordinates coords)
    {
        var flags = LookupFlags.Static | LookupFlags.Sundries | LookupFlags.Sensors;
        var lookup = _lookup.GetEntitiesInRange(coords, 0.5f, flags);
        foreach (var ent in lookup)
        {
            if (_tag.HasTag(ent, CarvingTag))
                return true;
        }

        return false;
    }

    private void OnCarvingSelected(Entity<CarvingKnifeComponent> ent, ref RuneCarvingSelectedMessage args)
    {
        var (uid, comp) = ent;

        if (!comp.Carvings.Contains(args.ProtoId))
            return;

        if (!HasComp<HereticComponent>(args.Actor) && !HasComp<GhoulComponent>(args.Actor))
            return;

        UpdateRunes(ent);

        if (comp.DrawnRunes.Count >= comp.MaxRuneAmount)
        {
            _popup.PopupEntity(Loc.GetString("carving-knife-comp-too-many-runes"), args.Actor, args.Actor);
            return;
        }

        var xform = Transform(args.Actor);
        var mapCoords = _transform.GetMapCoordinates(args.Actor, xform);

        if (!CanDrawRune(args.Actor, mapCoords))
        {
            _popup.PopupEntity(Loc.GetString("carving-knife-comp-cant-draw"), args.Actor, args.Actor);
            return;
        }

        if (CheckOtherCarvingsNearby(mapCoords))
        {
            _popup.PopupEntity(Loc.GetString("carving-knife-comp-close-to-another-carving"), args.Actor, args.Actor);
            return;
        }

        var doArgs = new DoAfterArgs(EntityManager,
            args.Actor,
            comp.RuneDrawTime,
            new CarveRuneDoAfterEvent(args.ProtoId),
            uid,
            used: uid)
        {
            NeedHand = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnWeightlessMove = false,
        };

        if (!_doAfter.TryStartDoAfter(doArgs))
            return;

        _audio.PlayPvs(comp.Sound, xform.Coordinates);
    }
}

[ByRefEvent]
public readonly record struct RuneCarvedEvent(EntityUid User);
