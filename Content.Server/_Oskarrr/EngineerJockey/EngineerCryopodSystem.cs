// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._Oskarrr.EngineerJockey;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Oskarrr.EngineerJockey;

public sealed class EngineerCryopodSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private static readonly SoundSpecifier AwakenSound =
        new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EngineerCryopodComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<EngineerCryopodComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<EngineerCryopodComponent, EngineerCryopodAwakenDoAfterEvent>(OnAwakenDoAfter);
        SubscribeLocalEvent<EngineerCryopodComponent, ActivateInWorldEvent>(OnActivate);

        SubscribeLocalEvent<EngineerConsoleComponent, BoundUIOpenedEvent>(OnConsoleUiOpened);
        SubscribeLocalEvent<EngineerConsoleComponent, EngineerConsoleAwakenMessage>(OnConsoleAwaken);
    }

    private void OnExamined(Entity<EngineerCryopodComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Awakened || !ent.Comp.Occupied)
        {
            args.PushMarkup(Loc.GetString("engineer-cryopod-examine-empty"));
            return;
        }

        args.PushMarkup(Loc.GetString("engineer-cryopod-examine-occupied"));
    }

    private void OnActivate(Entity<EngineerCryopodComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (!CanAwaken(ent, args.User, out var reason))
        {
            if (reason != null)
                _popup.PopupEntity(reason, ent, args.User);
            return;
        }

        args.Handled = true;
        TryBeginAwaken(args.User, ent);
    }

    private void OnGetVerbs(Entity<EngineerCryopodComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!CanAwaken(ent, args.User, out _))
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("engineer-cryopod-verb-awaken"),
            Act = () => TryBeginAwaken(user, ent),
            Priority = 10,
        });
    }

    public bool CanAwaken(Entity<EngineerCryopodComponent> pod, EntityUid user, out string? reason)
    {
        reason = null;

        if (pod.Comp.Awakened || !pod.Comp.Occupied)
        {
            reason = Loc.GetString("engineer-cryopod-already-empty");
            return false;
        }

        if (TerminatingOrDeleted(pod) || TerminatingOrDeleted(user))
            return false;

        return true;
    }

    public bool TryBeginAwaken(EntityUid user, Entity<EngineerCryopodComponent> pod)
    {
        if (!CanAwaken(pod, user, out var reason))
        {
            if (reason != null)
                _popup.PopupEntity(reason, pod, user);
            return false;
        }

        _popup.PopupEntity(Loc.GetString("engineer-cryopod-risk-warning"), user, user, PopupType.LargeCaution);

        var args = new DoAfterArgs(EntityManager, user, pod.Comp.AwakenDelay, new EngineerCryopodAwakenDoAfterEvent(), pod, pod)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        return _doAfter.TryStartDoAfter(args);
    }

    private void OnAwakenDoAfter(Entity<EngineerCryopodComponent> ent, ref EngineerCryopodAwakenDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;
        Awaken(ent, args.User);
    }

    public bool Awaken(Entity<EngineerCryopodComponent> pod, EntityUid? user)
    {
        if (pod.Comp.Awakened || !pod.Comp.Occupied)
            return false;

        var spawnCoords = _transform.GetMoverCoordinates(pod);
        var rot = Transform(pod).LocalRotation;
        var parent = Transform(pod).ParentUid;
        var spawnProto = pod.Comp.SpawnPrototype;
        var brokenProto = pod.Comp.BrokenPrototype;
        var podString = ToPrettyString(pod);

        pod.Comp.Occupied = false;
        pod.Comp.Awakened = true;
        Dirty(pod);

        var jockey = Spawn(spawnProto, spawnCoords);

        if (brokenProto is { } broken)
        {
            QueueDel(pod);
            var replacement = Spawn(broken, spawnCoords);
            _transform.SetLocalRotation(replacement, rot);
            if (parent.IsValid())
                _transform.SetParent(replacement, parent);
        }

        _audio.PlayPvs(AwakenSound, spawnCoords);

        if (user != null)
        {
            _adminLog.Add(LogType.Action, LogImpact.Extreme,
                $"{ToPrettyString(user.Value):user} awakened Engineer Jockey from {podString} → {ToPrettyString(jockey)}");
            _popup.PopupEntity(Loc.GetString("engineer-cryopod-awaken-success"), user.Value, user.Value, PopupType.LargeCaution);
        }

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString("engineer-cryopod-announcement"),
            Loc.GetString("engineer-cryopod-announcement-sender"),
            playSound: true,
            colorOverride: Color.OrangeRed);

        return true;
    }

    private void OnConsoleUiOpened(Entity<EngineerConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateConsoleUi(ent);
    }

    private void OnConsoleAwaken(Entity<EngineerConsoleComponent> ent, ref EngineerConsoleAwakenMessage args)
    {
        var user = args.Actor;
        if (!Exists(user))
            return;

        if (!TryGetEntity(args.Pod, out var podEnt) || !TryComp(podEnt, out EngineerCryopodComponent? cryo))
        {
            _popup.PopupEntity(Loc.GetString("engineer-console-pod-missing"), ent, user);
            return;
        }

        var pod = new Entity<EngineerCryopodComponent>(podEnt.Value, cryo);
        if (!CanAwaken(pod, user, out var reason))
        {
            if (reason != null)
                _popup.PopupEntity(reason, ent, user);
            UpdateConsoleUi(ent);
            return;
        }

        if (!_transform.InRange(ent.Owner, podEnt.Value, ent.Comp.Range))
        {
            _popup.PopupEntity(Loc.GetString("engineer-console-pod-out-of-range"), ent, user);
            return;
        }

        TryBeginAwaken(user, pod);
        UpdateConsoleUi(ent);
    }

    public void UpdateConsoleUi(EntityUid console)
    {
        if (!TryComp(console, out EngineerConsoleComponent? consoleComp))
            return;

        var range = consoleComp.Range;
        var entries = new List<EngineerCryopodEntry>();
        var origin = _transform.GetMapCoordinates(console);

        var query = EntityQueryEnumerator<EngineerCryopodComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var cryo, out var xform))
        {
            var mapCoords = _transform.GetMapCoordinates(uid, xform);
            if (mapCoords.MapId != origin.MapId)
                continue;

            if (!origin.InRange(mapCoords, range))
                continue;

            entries.Add(new EngineerCryopodEntry(
                GetNetEntity(uid),
                Name(uid),
                cryo.Occupied && !cryo.Awakened));
        }

        entries = entries.OrderByDescending(e => e.Occupied).ThenBy(e => e.Name).ToList();
        _ui.SetUiState(console, EngineerConsoleUiKey.Key, new EngineerConsoleBuiState(entries));
    }

    /// <summary>
    /// Used by AIMax: awaken the nearest occupied cryopod on the same map within range.
    /// </summary>
    public bool TryAwakenNearest(EntityUid user, float range, out EntityUid? pod)
    {
        pod = null;
        var origin = _transform.GetMapCoordinates(user);
        EntityUid? best = null;
        var bestDist = float.MaxValue;
        EngineerCryopodComponent? bestComp = null;

        var query = EntityQueryEnumerator<EngineerCryopodComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var cryo, out var xform))
        {
            if (!cryo.Occupied || cryo.Awakened)
                continue;

            var mapCoords = _transform.GetMapCoordinates(uid, xform);
            if (mapCoords.MapId != origin.MapId)
                continue;

            var dist = (mapCoords.Position - origin.Position).Length();
            if (dist > range || dist >= bestDist)
                continue;

            bestDist = dist;
            best = uid;
            bestComp = cryo;
        }

        if (best == null || bestComp == null)
            return false;

        pod = best;
        return TryBeginAwaken(user, (best.Value, bestComp));
    }
}
