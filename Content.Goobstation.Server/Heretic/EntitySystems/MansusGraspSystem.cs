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

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Server.Heretic.Abilities;
using Content.Goobstation.Server.Heretic.Components;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
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
using Content.Shared.Magic;
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

namespace Content.Goobstation.Server.Heretic.EntitySystems;

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
    [Dependency] private readonly SharedMagicSystem _magic = default!;

    public static readonly SoundSpecifier DefaultSound = new SoundPathSpecifier("/Audio/Items/welder.ogg");

    public static readonly LocId DefaultInvocation = "heretic-speech-mansusgrasp";

    public static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(10);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MansusGraspComponent, AfterInteractEvent>(OnAfterInteract);
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

    private void OnRustInteract(Entity<RustGraspComponent> rustGrasp, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach
            || !TryComp<HereticComponent>(args.User, out var heretic)
            || !TryComp<UseDelayComponent>(rustGrasp, out var delay)
            || !TryComp<MansusGraspComponent>(rustGrasp, out var graspComponent)
            || _delay.IsDelayed((rustGrasp.Owner, delay), rustGrasp.Comp.DelayId))
            return;

        if (args.Target == null
            || _whitelist.IsBlacklistPass(graspComponent.Blacklist, args.Target.Value)
            || !TryRustCoordinates(rustGrasp.Comp, args.ClickLocation))
        {
            ResetDelay(heretic, rustGrasp);
            InvokeGrasp(args.User, (rustGrasp, graspComponent));

            return;
        }

        // Already rusted walls are destroyed
        if (HasComp<RustedWallComponent>(args.Target))
        {
            if (!_ability.CanSurfaceBeRusted(args.Target.Value, (args.User, heretic)))
                return;

            args.Handled = true;
            InvokeGrasp(args.User, (rustGrasp.Owner, graspComponent));
            ResetDelay(heretic, rustGrasp);
            Del(args.Target);
            return;
        }

        // Death to catwalks
        if (_tag.HasTag(args.Target.Value, rustGrasp.Comp.CatwalkTag))
        {
            args.Handled = true;
            InvokeGrasp(args.User, (rustGrasp, graspComponent));
            ResetDelay(heretic, rustGrasp, rustGrasp.Comp.CatwalkDelayMultiplier);
            Del(args.Target);
            return;
        }

        if (!_ability.TryMakeRustWall(args.Target.Value, (args.User, heretic)))
            return;

        args.Handled = true;
        InvokeGrasp(args.User, (rustGrasp.Owner, graspComponent));
        ResetDelay(heretic, rustGrasp);
    }

    private void OnAfterInteract(Entity<MansusGraspComponent> grasp, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target is not { } target
            || args.Target == args.User)
            return;

        if (!TryComp<HereticComponent>(args.User, out var hereticComp))
        {
            QueueDel(grasp);
            args.Handled = true;
            return;
        }

        if (TryComp<HereticComponent>(target, out var targetHeretic)
            && targetHeretic.CurrentPath == grasp.Comp.Path // Don't affect same path heretics.
            && _whitelist.IsBlacklistPass(grasp.Comp.Blacklist, target)) // Don't affect blacklisted.
            return;

        var beforeEvent = new BeforeHarmfulActionEvent(args.User, HarmfulActionType.MansusGrasp);
        RaiseLocalEvent(target, beforeEvent);

        var cancelled = beforeEvent.Cancelled;
        if (!cancelled)
        {
            var ev = new BeforeCastTouchSpellEvent(args.Target.Value);
            RaiseLocalEvent(target, ev, true);
            cancelled = ev.Cancelled;
        }

        if (cancelled)
        {
            _actions.SetCooldown(hereticComp.MansusGrasp, grasp.Comp.CooldownAfterUse);
            hereticComp.MansusGrasp = null;
            InvokeGrasp(args.User, grasp);
            QueueDel(grasp);
            args.Handled = true;
            return;
        }

        // upgraded grasp
        if (!TryApplyGraspEffectAndMark(args.User, hereticComp, target, grasp))
            return;

        if (TryComp<StatusEffectsComponent>(target, out var status))
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
        InvokeGrasp(args.User, grasp);
        QueueDel(grasp);
        args.Handled = true;
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

        var runeProto = heretic.RuneDrawAnimationProto;
        float time = 14;

        if (TryComp(ent, out TransmutationRuneScriberComponent? scriber)) // Used for the Codex Cicatrix.
        {
            runeProto = scriber.RuneDrawingEntity;
            time = scriber.Time;
        }
        else if (heretic.MansusGrasp == null
                 || !tags.SetEquals(heretic.ValidDrawingUtensilTags))
            return;

        args.Handled = true;

        // Remove our rune if clicked/
        if (args.Target != null
            && HasComp<HereticRitualRuneComponent>(args.Target))
        {
            // todo: add more fluff
            QueueDel(args.Target);
            return;
        }

        // spawn our rune
        var rune = Spawn(runeProto, args.ClickLocation);
        _transform.AttachToGridOrMap(rune);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            time,
            new DrawRitualRuneDoAfterEvent(rune, args.ClickLocation),
            args.User)
        {
            BreakOnDamage = true,
            BreakOnHandChange = true,
            BreakOnMove = true,
            CancelDuplicate = false,
            MultiplyDelay = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
    private void OnRitualRuneDoAfter(Entity<HereticComponent> ent, ref DrawRitualRuneDoAfterEvent ev)
    {
        QueueDel(ev.RitualRune);

        if (!ev.Cancelled)
            _transform.AttachToGridOrMap(Spawn("HereticRuneRitual", ev.Coords));
    }

    #region Helpers

    private bool TryRustCoordinates(RustGraspComponent grasp, EntityCoordinates target)
    {
        if (!target.IsValid(EntityManager)
            || !_mapManager.TryFindGridAt(_transform.ToMapCoordinates(target), out var gridUid, out var mapGrid))
            return false;

        var tileRef = _mapSystem.GetTileRef(gridUid, mapGrid, target);
        var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];

        if (!_ability.CanRustTile(tileDef))
            return false;

        _ability.MakeRustTile(gridUid, mapGrid, tileRef, grasp.TileRune);
        return true;
    }

    private void ResetDelay(HereticComponent heretic, Entity<RustGraspComponent> grasp, float multiplier = 1f, UseDelayComponent? useDelay = null)
    {
        if (!Resolve(grasp, ref useDelay))
            return;

        // Less delay the higher the path stage is
        var length = float.Lerp(grasp.Comp.MaxUseDelay, grasp.Comp.MinUseDelay, heretic.PathStage / 10f) * multiplier;
        _delay.SetLength((grasp, useDelay), TimeSpan.FromSeconds(length), grasp.Comp.DelayId);
        _delay.TryResetDelay((grasp, useDelay), false, grasp.Comp.DelayId);
    }

    #endregion
}
