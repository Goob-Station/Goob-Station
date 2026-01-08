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

using System.Linq;
using Content.Goobstation.Common.Religion;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Heretic.Abilities;
using Content.Server.Heretic.Components;
using Content.Server.Heretic.Components.PathSpecific;
using Content.Server.Popups;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Server.Heretic.EntitySystems;

public sealed class MansusGraspSystem : SharedMansusGraspSystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RatvarianLanguageSystem _language = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly HereticAbilitySystem _ability = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public static readonly SoundSpecifier DefaultSound = new SoundPathSpecifier("/Audio/Items/welder.ogg");

    public static readonly LocId DefaultInvocation = "heretic-speech-mansusgrasp";

    public static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(10);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MansusGraspComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MansusGraspComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<TagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<RustGraspComponent, AfterInteractEvent>(OnRustInteract);
        SubscribeLocalEvent<HereticComponent, DrawRitualRuneDoAfterEvent>(OnRitualRuneDoAfter);
        SubscribeLocalEvent<MansusGraspBlockTriggerComponent, BeforeTriggerEvent>(OnTriggerAttempt);
    }

    private void OnTriggerAttempt(Entity<MansusGraspBlockTriggerComponent> ent, ref BeforeTriggerEvent args)
    {
        if (HasComp<MansusGraspAffectedComponent>(args.User))
        {
            args.Cancel();
            _popup.PopupEntity(Loc.GetString("mansus-grasp-trigger-fail"), args.User.Value, args.User.Value);
        }
        else if (HasComp<MansusGraspAffectedComponent>(Transform(ent).ParentUid))
            args.Cancel();
    }

    private void OnRustInteract(EntityUid uid, RustGraspComponent comp, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || !TryComp<HereticComponent>(args.User, out var heretic) ||
            !TryComp(uid, out UseDelayComponent? delay) || _delay.IsDelayed((uid, delay), comp.Delay) ||
            !TryComp(uid, out MansusGraspComponent? grasp))
            return;

        if (args.Target == null || _whitelist.IsBlacklistPass(grasp.Blacklist, args.Target.Value))
        {
            RustTile();
            return;
        }

        // Death to catwalks
        if (_tag.HasTag(args.Target.Value, "Catwalk"))
        {
            args.Handled = true;
            InvokeGrasp(args.User, (uid, grasp));
            ResetDelay(comp.CatwalkDelayMultiplier);
            Del(args.Target);
            return;
        }

        if (!_ability.TryMakeRustWall(args.Target.Value, (args.User, heretic)))
            return;

        args.Handled = true;
        InvokeGrasp(args.User, (uid, grasp));
        ResetDelay();

        return;

        void RustTile()
        {
            if (!args.ClickLocation.IsValid(EntityManager))
                return;

            if (!_mapManager.TryFindGridAt(_transform.ToMapCoordinates(args.ClickLocation), out var gridUid, out var mapGrid))
                return;

            var tileRef = _mapSystem.GetTileRef(gridUid, mapGrid, args.ClickLocation);
            var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];

            if (!_ability.CanRustTile(tileDef))
                return;

            args.Handled = true;
            ResetDelay();
            InvokeGrasp(args.User, (uid, grasp));

            _ability.MakeRustTile(gridUid, mapGrid, tileRef, comp.TileRune);
        }

        void ResetDelay(float multiplier = 1f)
        {
            // Less delay the higher the path stage is
            var length = float.Lerp(comp.MaxUseDelay, comp.MinUseDelay, heretic.PathStage / 10f) * multiplier;
            _delay.SetLength((uid, delay), TimeSpan.FromSeconds(length), comp.Delay);
            _delay.TryResetDelay((uid, delay), false, comp.Delay);
        }
    }

    private bool GraspTarget(Entity<MansusGraspComponent> grasp, EntityUid user, EntityUid target)
    {
        if (!TryComp<HereticComponent>(user, out var hereticComp))
        {
            QueueDel(grasp);
            return true;
        }

        if (_whitelist.IsBlacklistPass(grasp.Comp.Blacklist, target))
            return false;

        var beforeEvent = new BeforeHarmfulActionEvent(user, HarmfulActionType.MansusGrasp);
        RaiseLocalEvent(target, beforeEvent);
        var cancelled = beforeEvent.Cancelled;
        if (!cancelled)
        {
            var ev = new BeforeCastTouchSpellEvent(target);
            RaiseLocalEvent(target, ev, true);
            cancelled = ev.Cancelled;
        }

        if (cancelled)
        {
            _actions.SetCooldown(hereticComp.MansusGrasp, grasp.Comp.CooldownAfterUse);
            hereticComp.MansusGrasp = EntityUid.Invalid;
            InvokeGrasp(user, grasp);
            QueueDel(grasp);
            return true;
        }

        // upgraded grasp
        if (!TryApplyGraspEffectAndMark(user, hereticComp, target, grasp, out var triggerGrasp))
            return false;

        if (triggerGrasp && TryComp(target, out StatusEffectsComponent? status))
        {
            _stun.KnockdownOrStun(target, grasp.Comp.KnockdownTime, true, status);
            _stamina.TakeStaminaDamage(target, grasp.Comp.StaminaDamage);
            _language.DoRatvarian(target, grasp.Comp.SpeechTime, true, status);
            _statusEffect.TryAddStatusEffect<MansusGraspAffectedComponent>(target,
                "MansusGraspAffected",
                grasp.Comp.AffectedTime,
                true,
                status);
        }

        _actions.SetCooldown(hereticComp.MansusGrasp, grasp.Comp.CooldownAfterUse);
        hereticComp.MansusGrasp = EntityUid.Invalid;
        InvokeGrasp(user, grasp);
        QueueDel(grasp);
        return true;
    }

    private void OnMelee(Entity<MansusGraspComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;
        // blocked from wide attacks in YAML. should never have more than 1
        if (args.HitEntities.Count > 1)
            return;
        var target = args.HitEntities.First();
        // no fumbling!
        if (target == args.User)
            return;
        args.Handled = GraspTarget(ent, args.User,target);
    }

    private void OnAfterInteract(Entity<MansusGraspComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        args.Handled = GraspTarget(ent, args.User,args.Target.Value);
    }

    public void InvokeGrasp(EntityUid user, Entity<MansusGraspComponent>? ent)
    {
        var (sound, invocation) = ent == null
            ? (DefaultSound, DefaultInvocation)
            : (ent.Value.Comp.Sound, ent.Value.Comp.Invocation);

        _audio.PlayPvs(sound, user);
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
