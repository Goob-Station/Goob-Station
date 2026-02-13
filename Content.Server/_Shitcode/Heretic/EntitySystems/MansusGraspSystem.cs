// SPDX-FileCopyrightText: 2024 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 github-actions <github-actions@github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Content.Shared.Trigger;

namespace Content.Server.Heretic.EntitySystems;

public sealed class MansusGraspSystem : SharedMansusGraspSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public static readonly LocId DefaultInvocation = "heretic-speech-mansusgrasp";

    public static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(10);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HereticComponent, DrawRitualRuneDoAfterEvent>(OnRitualRuneDoAfter);
        SubscribeLocalEvent<MansusGraspBlockTriggerComponent, AttemptTriggerEvent>(OnTriggerAttempt);
    }

    private void OnTriggerAttempt(Entity<MansusGraspBlockTriggerComponent> ent, ref AttemptTriggerEvent args)
    {
        if (HasComp<MansusGraspAffectedComponent>(args.User))
        {
            args.Cancelled = true;
            _popup.PopupEntity(Loc.GetString("mansus-grasp-trigger-fail"), args.User.Value, args.User.Value);
        }
        else if (HasComp<MansusGraspAffectedComponent>(Transform(ent).ParentUid))
            args.Cancelled = true;
    }

    public override void InvokeGrasp(EntityUid user, Entity<MansusGraspComponent>? ent)
    {
        base.InvokeGrasp(user, ent);

        var invocation = ent == null ? DefaultInvocation : ent.Value.Comp.Invocation;
        _chat.TrySendInGameICMessage(user, Loc.GetString(invocation), InGameICChatType.Speak, false);
    }

    private void OnAfterInteract(Entity<TagComponent> ent, ref AfterInteractEvent args)
    {
        var tags = ent.Comp.Tags;

        if (!args.CanReach
            || !args.ClickLocation.IsValid(EntityManager)
            || !TryComp<HereticComponent>(args.User, out var heretic) // not a heretic - how???
            || HasComp<ActiveDoAfterComponent>(args.User)) // prevent rune shittery
            return;

        var runeProto = "HereticRuneRitualDrawAnimation";
        float time = 14;

        if (TryComp(ent, out TransmutationRuneScriberComponent? scriber)) // if it is special rune scriber
        {
            runeProto = scriber.RuneDrawingEntity;
            time = scriber.Time;
        }
        else if (heretic.MansusGrasp == EntityUid.Invalid // no grasp - not special
                 || !tags.Contains("Write") || !tags.Contains("Pen")) // not a pen
            return;

        args.Handled = true;

        // remove our rune if clicked
        if (args.Target != null && HasComp<HereticRitualRuneComponent>(args.Target))
        {
            // todo: add more fluff
            QueueDel(args.Target);
            return;
        }

        // spawn our rune
        var rune = Spawn(runeProto, args.ClickLocation);
        _transform.AttachToGridOrMap(rune);
        var dargs = new DoAfterArgs(EntityManager, args.User, time, new DrawRitualRuneDoAfterEvent(rune, args.ClickLocation), args.User)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            CancelDuplicate = false,
            MultiplyDelay = false,
        };
        _doAfter.TryStartDoAfter(dargs);
    }
    private void OnRitualRuneDoAfter(Entity<HereticComponent> ent, ref DrawRitualRuneDoAfterEvent ev)
    {
        // delete the animation rune regardless
        QueueDel(ev.RitualRune);

        if (!ev.Cancelled)
            _transform.AttachToGridOrMap(Spawn("HereticRuneRitual", ev.Coords));
    }
}
