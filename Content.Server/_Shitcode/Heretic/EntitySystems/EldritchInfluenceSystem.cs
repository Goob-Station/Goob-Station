// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Server.Chat.Managers;
using Content.Server.Heretic.Components;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server.Heretic.EntitySystems;

public sealed class EldritchInfluenceSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doafter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    [Dependency] private readonly IChatManager _chatMan = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EldritchInfluenceComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<EldritchInfluenceComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<EldritchInfluenceComponent, EldritchInfluenceDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<EldritchInfluenceComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<EldritchInfluenceComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<HereticComponent>(args.Examiner) || HasComp<GhoulComponent>(args.Examiner) ||
            HasComp<SpectralComponent>(args.Examiner) || HasComp<GhostComponent>(args.Examiner) |
            HasComp<WizardComponent>(args.Examiner) || HasComp<ApprenticeComponent>(args.Examiner))
            return;

        if (!_mind.TryGetMind(args.Examiner, out _, out var mind))
            return;

        if (!_playerMan.TryGetSessionById(mind.UserId, out var session))
            return;

        _audio.PlayGlobal(ent.Comp.ExamineSound, session);

        var baseMessage = ent.Comp.ExamineBaseMessage;
        var message = Loc.GetString(_random.Pick(ent.Comp.HeathenExamineMessages));
        var size = ent.Comp.FontSize;
        var loc = Loc.GetString(baseMessage, ("size", size), ("text", message));
        SharedChatSystem.UpdateFontSize(size, ref message, ref loc);
        _chatMan.ChatMessageToOne(ChatChannel.Server, message, loc, default, false, session.Channel, canCoalesce: false);

        var effectArgs = new EntityEffectBaseArgs(args.Examiner, EntityManager);
        var effects = _random.Pick(ent.Comp.PossibleExamineEffects);
        foreach (var effect in effects)
        {
            if (effect.ShouldApply(effectArgs, _random))
                effect.Effect(effectArgs);
        }
    }

    public bool CollectInfluence(Entity<EldritchInfluenceComponent> influence, Entity<HereticComponent> user, EntityUid? used = null)
    {
        if (influence.Comp.Spent)
            return false;

        var (time, hidden) = TryComp<EldritchInfluenceDrainerComponent>(used, out var drainer)
            ? (drainer.Time, drainer.Hidden)
            : (10f, true);

        var doAfter = new EldritchInfluenceDoAfterEvent();
        var dargs = new DoAfterArgs(EntityManager, user, time, doAfter, influence, influence, used)
        {
            NeedHand = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            MultiplyDelay = false,
            Hidden = hidden,
        };
        _popup.PopupEntity(Loc.GetString("heretic-influence-start"), influence, user);
        return _doafter.TryStartDoAfter(dargs);
    }

    private void OnInteract(Entity<EldritchInfluenceComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled
        || !TryComp<HereticComponent>(args.User, out var heretic))
            return;

        args.Handled = CollectInfluence(ent, (args.User, heretic));
    }
    private void OnInteractUsing(Entity<EldritchInfluenceComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled
        || !TryComp<HereticComponent>(args.User, out var heretic))
            return;

        args.Handled = CollectInfluence(ent, (args.User, heretic), args.Used);
    }
    private void OnDoAfter(Entity<EldritchInfluenceComponent> ent, ref EldritchInfluenceDoAfterEvent args)
    {
        if (args.Cancelled
        || args.Target == null
        || !TryComp<HereticComponent>(args.User, out var heretic))
            return;

        var knowledge = TryComp(args.Used, out EldritchInfluenceDrainerComponent? drainer)
            ? drainer.KnowledgePerInfluence
            : 1f;

        _heretic.UpdateKnowledge(args.User, heretic, knowledge);

        Spawn("EldritchInfluenceIntermediate", Transform(args.Target.Value).Coordinates);
        QueueDel(args.Target);
    }
}
