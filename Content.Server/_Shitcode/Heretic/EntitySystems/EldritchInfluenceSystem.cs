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

using Content.Server.Heretic.Components;
using Content.Server.Popups;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;
using Content.Shared.Interaction;

namespace Content.Server.Heretic.EntitySystems;

public sealed class EldritchInfluenceSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doafter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EldritchInfluenceComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<EldritchInfluenceComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<EldritchInfluenceComponent, EldritchInfluenceDoAfterEvent>(OnDoAfter);
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