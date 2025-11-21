using Content.Goobstation.Shared.PhaseShift;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Movement.Pulling.Events;
using Robust.Shared.Network;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Tag;
using Content.Shared.Doors.Systems;
using Content.Shared.Hands;
using Content.Goobstation.Common.Footprints;
using Content.Shared.Movement.Components;
using Content.Shared.Speech.Muting;
using Content.Shared.Emoting;
using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Temperature.Components;
using Content.Goobstation.Shared.Body.Components;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Ghost;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Humanoid;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed class SlasherIncorporealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private const string FootstepSoundTag = "FootstepSound";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlasherIncorporealComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherIncorporealComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherIncorporealizeEvent>(OnIncorporealize);
        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherCorporealizeEvent>(OnCorporealize);
        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherIncorporealizeDoAfterEvent>(OnIncorporealizeDoAfter);
        SubscribeLocalEvent<SlasherIncorporealComponent, SlasherIncorporealObserverCheckEvent>(OnObserverCheck);

        SubscribeLocalEvent<SlasherIncorporealComponent, InteractionAttemptEvent>(OnAttemptInteract);
        SubscribeLocalEvent<SlasherIncorporealComponent, InteractionVerbAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, UseAttemptEvent>(OnUseAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, GettingAttackedAttemptEvent>(OnGettingAttackedAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, BeforeDamageChangedEvent>(OnBeforeDamage);
        SubscribeLocalEvent<SlasherIncorporealComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<SlasherIncorporealComponent, BeforeEmoteEvent>(OnBeforeEmote);
        SubscribeLocalEvent<SlasherIncorporealComponent, EmoteAttemptEvent>(OnEmoteAttempt);
        SubscribeLocalEvent<SlasherIncorporealComponent, FootprintLeaveAttemptEvent>(OnFootprintLeaveAttempt);

        SubscribeLocalEvent<ActionComponent, ActionAttemptEvent>(OnAnyActionAttempt);
    }

    private void OnMapInit(Entity<SlasherIncorporealComponent> ent, ref MapInitEvent args)
    {
        if (!_net.IsServer)
            return;

        _actions.AddAction(ent.Owner, ref ent.Comp.IncorporealizeActionEnt, ent.Comp.IncorporealizeActionId);
        _actions.AddAction(ent.Owner, ref ent.Comp.CorporealizeActionEnt, ent.Comp.CorporealizeActionId);
        _actions.SetEnabled(ent.Comp.CorporealizeActionEnt, false);

    }

    private void OnShutdown(Entity<SlasherIncorporealComponent> ent, ref ComponentShutdown args)
    {
        if (!_net.IsServer)
            return;

        _actions.RemoveAction(ent.Owner, ent.Comp.IncorporealizeActionEnt);
        _actions.RemoveAction(ent.Owner, ent.Comp.CorporealizeActionEnt);
    }

    private void OnIncorporealize(Entity<SlasherIncorporealComponent> ent, ref SlasherIncorporealizeEvent args)
    {
        if (args.Handled || ent.Comp.IsIncorporeal)
            return;

        // Run server-side only to avoid client prediction flicker
        if (!_net.IsServer)
        {
            args.Handled = true;
            return;
        }

        // Check if anyone can see them.
        var checkEv = new SlasherIncorporealObserverCheckEvent(GetNetEntity(ent.Owner), ent.Comp.ObserverCheckRange);
        RaiseLocalEvent(ent.Owner, checkEv);
        if (checkEv.Cancelled)
        {
            _popup.PopupEntity(Loc.GetString("slasher-corporealize-fail-seen"), ent.Owner, ent.Owner);
            return;
        }

        // Start do-after to enter incorporeal
        var doArgs = new DoAfterArgs(EntityManager,
            ent.Owner,
            ent.Comp.IncorporealizeDelay,
            new SlasherIncorporealizeDoAfterEvent(),
            ent.Owner,
            target: ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BlockDuplicate = true,
            Hidden = false,
            DistanceThreshold = 1f,
        };
        _doAfter.TryStartDoAfter(doArgs);

        args.Handled = true;
    }

    private void OnCorporealize(Entity<SlasherIncorporealComponent> ent, ref SlasherCorporealizeEvent args)
    {
        if (args.Handled)
            return;

        if (!_net.IsServer)
        {
            args.Handled = true;
            return;
        }

        if (!ent.Comp.IsIncorporeal)
            return;
        ExitIncorporeal(ent.Owner, ent);
        args.Handled = true;
    }

    private void OnIncorporealizeDoAfter(Entity<SlasherIncorporealComponent> ent, ref SlasherIncorporealizeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_net.IsServer)
        {
            args.Handled = true;
            return;
        }

        // Check if anyone can see them.
        var checkEv = new SlasherIncorporealObserverCheckEvent(GetNetEntity(ent.Owner), ent.Comp.ObserverCheckRange);
        RaiseLocalEvent(ent.Owner, checkEv);
        if (checkEv.Cancelled)
        {
            _popup.PopupEntity(Loc.GetString("slasher-corporealize-fail-seen"), ent.Owner, ent.Owner);
            return;
        }

        EnterIncorporeal(ent.Owner, ent);
        args.Handled = true;
    }

    private void EnterIncorporeal(EntityUid uid, Entity<SlasherIncorporealComponent> ent)
    {
        ent.Comp.IsIncorporeal = true;
        Dirty(ent);

        // main component.
        var phase = EnsureComp<PhaseShiftedComponent>(uid);
        phase.MovementSpeedBuff = 3.5f;

        // don't wanna let people see them obviously.
        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, stealth.MinVisibility, stealth);

        _actions.SetEnabled(ent.Comp.IncorporealizeActionEnt, false);
        _actions.SetEnabled(ent.Comp.CorporealizeActionEnt, true);

        // Prevent doors from opening.
        if (_tags.HasTag(uid, SharedDoorSystem.DoorBumpTag))
            _tags.RemoveTag(uid, SharedDoorSystem.DoorBumpTag);

        // Remove footstep sounds while incorporeal.
        if (_tags.HasTag(uid, FootstepSoundTag))
            _tags.RemoveTag(uid, FootstepSoundTag);

        // Do not leave puddle footsteps while incorporeal (handled via event instead of component removal).

        // Mute and block vocal emotes.
        _ = EnsureComp<MutedComponent>(uid);

        // Disable FOV for full vision while incorporeal.
        _eye.SetDrawFov(uid, false);

        // Space immunity
        _ = EnsureComp<MovementIgnoreGravityComponent>(uid);
        _ = EnsureComp<SpecialPressureImmunityComponent>(uid);
        _ = EnsureComp<SpecialBreathingImmunityComponent>(uid);
        _ = EnsureComp<SpecialLowTempImmunityComponent>(uid);
        _ = EnsureComp<SpecialHighTempImmunityComponent>(uid);
    }

    private void ExitIncorporeal(EntityUid uid, Entity<SlasherIncorporealComponent> ent)
    {
        ent.Comp.IsIncorporeal = false;
        Dirty(ent);
        if (HasComp<PhaseShiftedComponent>(uid))
            RemComp<PhaseShiftedComponent>(uid);

        if (HasComp<StealthComponent>(uid))
            RemComp<StealthComponent>(uid);
        _actions.SetEnabled(ent.Comp.IncorporealizeActionEnt, true);
        _actions.SetEnabled(ent.Comp.CorporealizeActionEnt, false);

        _tags.AddTag(uid, SharedDoorSystem.DoorBumpTag);

        _tags.AddTag(uid, FootstepSoundTag);


        // Let them speak
        _ = RemComp<MutedComponent>(uid);

        // Restore FOV
        _eye.SetDrawFov(uid, true);

        // Remove space immunity
        _ = RemComp<MovementIgnoreGravityComponent>(uid);
        _ = RemComp<SpecialPressureImmunityComponent>(uid);
        _ = RemComp<SpecialBreathingImmunityComponent>(uid);
        _ = RemComp<SpecialLowTempImmunityComponent>(uid);
        _ = RemComp<SpecialHighTempImmunityComponent>(uid);
    }

    // Event hell below
    private void OnBeforeEmote(EntityUid uid, SlasherIncorporealComponent comp, ref BeforeEmoteEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }

    private void OnEmoteAttempt(EntityUid uid, SlasherIncorporealComponent comp, ref EmoteAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }

    private void OnAttempt(EntityUid uid, SlasherIncorporealComponent comp, CancellableEntityEventArgs args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }

    private void OnAttackAttempt(EntityUid uid, SlasherIncorporealComponent comp, AttackAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }

    private void OnUseAttempt(EntityUid uid, SlasherIncorporealComponent comp, UseAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }

    // Do note that you can grab someone that's in crit or dead and then use incorporealize to drag their body around while invisible but this is hilarious so I'm keeping it
    private void OnPullAttempt(EntityUid uid, SlasherIncorporealComponent comp, PullAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancelled = true;
    }

    private void OnGettingAttackedAttempt(EntityUid uid, SlasherIncorporealComponent comp, ref GettingAttackedAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancelled = true;
    }

    private void OnBeforeDamage(EntityUid uid, SlasherIncorporealComponent comp, ref BeforeDamageChangedEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancelled = true;
    }

    private void OnBeforeStaminaDamage(EntityUid uid, SlasherIncorporealComponent comp, ref BeforeStaminaDamageEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancelled = true;
    }

    private void OnDropAttempt(EntityUid uid, SlasherIncorporealComponent comp, DropAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }
    private void OnAttemptInteract(EntityUid uid, SlasherIncorporealComponent comp, ref InteractionAttemptEvent args)
    {
        // Allow self / action usages (target null) so corporealize can still be used
        if (comp.IsIncorporeal && args.Target != null)
            args.Cancelled = true;
    }

    private void OnFootprintLeaveAttempt(EntityUid uid, SlasherIncorporealComponent comp, ref FootprintLeaveAttemptEvent args)
    {
        if (comp.IsIncorporeal)
            args.Cancel();
    }

    private void OnAnyActionAttempt(Entity<ActionComponent> action, ref ActionAttemptEvent args)
    {
        var user = args.User;
        if (!TryComp<SlasherIncorporealComponent>(user, out var comp) || !comp.IsIncorporeal)
            return;

        // Allow only the slasher's "Corporealize" action while incorporeal.
        if (comp.CorporealizeActionEnt != action.Owner)
            args.Cancelled = true;
    }

    private void OnObserverCheck(EntityUid uid, SlasherIncorporealComponent comp, ref SlasherIncorporealObserverCheckEvent args)
    {
        // args.Range may differ from comp.ObserverCheckRange if needed.
        var checkRange = args.Range;
        foreach (var other in _lookup.GetEntitiesInRange(uid, checkRange))
        {
            if (other == uid || !HasComp<EyeComponent>(other))
                continue;

            if (HasComp<GhostComponent>(other))
                continue;

            if (!HasComp<HumanoidAppearanceComponent>(other))
                continue;

            if (_mobState.IsDead(other))
                continue;

            if (_mobState.IsCritical(other))
                continue;

            if (_interaction.InRangeUnobstructed(other, uid, checkRange, CollisionGroup.Opaque))
            {
                args.Cancelled = true;
                return;
            }
        }
    }
}
