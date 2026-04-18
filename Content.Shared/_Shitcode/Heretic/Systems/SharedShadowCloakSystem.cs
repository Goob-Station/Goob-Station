using Content.Goobstation.Common.Heretic;
using Content.Goobstation.Common.Identity;
using Content.Goobstation.Common.Speech;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Systems;
using Content.Shared.Rotation;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedShadowCloakSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifier = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    private EntityQuery<ShadowCloakEntityComponent> _cloakQuery;

    private static readonly ProtoId<TagPrototype> ActionTag = "ShadowCloakAction";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCloakedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowCloakedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ShadowCloakedComponent, GetDoAfterDelayMultiplierEvent>(OnGetDoAfterSpeed);
        SubscribeLocalEvent<ShadowCloakedComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<ShadowCloakedComponent, TransformSpeakerNameEvent>(OnTransformName);
        SubscribeLocalEvent<ShadowCloakedComponent, TryGetIdentityShortInfoEvent>(OnGetIdentity);
        SubscribeLocalEvent<ShadowCloakedComponent, GetIdentityRepresentationEntityEvent>(OnGetIdentityEntity);
        SubscribeLocalEvent<ShadowCloakedComponent, GetSpeechSoundEvent>(OnGetSpeechSound);
        SubscribeLocalEvent<ShadowCloakedComponent, GetEmoteSoundsEvent>(OnGetEmoteSound);
        SubscribeLocalEvent<ShadowCloakedComponent, GetBarkSourceEntityEvent>(OnGetBark);
        SubscribeLocalEvent<ShadowCloakedComponent, GetVirtualItemBlockingEntityEvent>(OnGetBlockingEntity);
        SubscribeLocalEvent<ShadowCloakedComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<ShadowCloakedComponent, StoodEvent>(OnStand);

        SubscribeLocalEvent<ShadowCloakEntityComponent, EntParentChangedMessage>(OnEntParentChanged);
        SubscribeLocalEvent<ShadowCloakEntityComponent, ComponentShutdown>(OnCloakShutdown);
        SubscribeLocalEvent<ShadowCloakEntityComponent, DamageChangedEvent>(OnDamage);

        _cloakQuery = GetEntityQuery<ShadowCloakEntityComponent>();
    }

    private void OnStand(Entity<ShadowCloakedComponent> ent, ref StoodEvent args)
    {
        if (GetShadowCloakEntity(ent) is { } cloak)
            _appearance.SetData(cloak, RotationVisuals.RotationState, RotationState.Vertical);
    }

    private void OnDowned(Entity<ShadowCloakedComponent> ent, ref DownedEvent args)
    {
        if (GetShadowCloakEntity(ent) is { } cloak)
            _appearance.SetData(cloak, RotationVisuals.RotationState, RotationState.Horizontal);
    }

    private void OnGetBlockingEntity(Entity<ShadowCloakedComponent> ent, ref GetVirtualItemBlockingEntityEvent args)
    {
        if (GetShadowCloakEntity(ent) is { } cloak)
            args.Uid = cloak;
    }

    private void OnGetBark(Entity<ShadowCloakedComponent> ent, ref GetBarkSourceEntityEvent args)
    {
        if (GetShadowCloakEntity(ent) is { } cloak)
            args.Ent = cloak;
    }

    private void OnGetEmoteSound(Entity<ShadowCloakedComponent> ent, ref GetEmoteSoundsEvent args)
    {
        if (args.Handled || GetShadowCloakEntity(ent) is not { } cloak)
            return;

        args.Handled = true;
        args.EmoteSoundProtoId = cloak.Comp.EmoteSounds;
    }

    private void OnGetSpeechSound(Entity<ShadowCloakedComponent> ent, ref GetSpeechSoundEvent args)
    {
        if (args.Handled || GetShadowCloakEntity(ent) is not { } cloak)
            return;

        args.Handled = true;
        args.SpeechSoundProtoId = cloak.Comp.SpeechSounds;
    }

    private void OnGetIdentityEntity(Entity<ShadowCloakedComponent> ent, ref GetIdentityRepresentationEntityEvent args)
    {
        if (GetShadowCloakEntity(ent) is { } cloak)
            args.Uid = cloak;
    }

    private void OnGetIdentity(Entity<ShadowCloakedComponent> ent, ref TryGetIdentityShortInfoEvent args)
    {
        if (GetShadowCloakEntity(ent) is { } cloak)
            args.Title = Name(cloak);
    }

    private void OnTransformName(Entity<ShadowCloakedComponent> ent, ref TransformSpeakerNameEvent args)
    {
        if (GetShadowCloakEntity(ent) is not { } cloak)
            return;

        args.SpeechVerb = cloak.Comp.SpeechVerb;
        args.VoiceName = Name(cloak);
    }

    private void OnDamage(Entity<ShadowCloakEntityComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.User is not {} user)
            return;

        if ((args.UncappedDamage ?? args.DamageDelta) is not { } dmg)
            return;

        _dmg.TryChangeDamage(user,
            dmg,
            origin: args.Origin,
            interruptsDoAfters: args.InterruptsDoAfters,
            ignoreBlockers: args.IgnoreBlockers,
            targetPart: TargetBodyPart.Vital,
            canMiss: false);
    }

    private void OnDamageChanged(Entity<ShadowCloakedComponent> ent, ref DamageChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        if (GetShadowCloakEntity(ent) is not { } cloak)
            return;

        cloak.Comp.SustainedDamage += args.DamageDelta.GetTotal();

        if (cloak.Comp.SustainedDamage < cloak.Comp.DamageBeforeReveal)
            return;

        var chance = Math.Clamp(cloak.Comp.SustainedDamage.Float() * cloak.Comp.RevealDamageMultiplier / 100f, 0f, 1f);
        if (!_random.Prob(chance))
            return;

        if (cloak.Comp.DebuffOnEarlyReveal)
        {
            _stun.KnockdownOrStun(ent, cloak.Comp.KnockdownTime);
            _status.TryUpdateStatusEffectDuration(ent, cloak.Comp.SlowdownEffect, cloak.Comp.SlowdownTime);
        }

        ResetAbilityCooldown(ent, cloak.Comp.ForceRevealCooldown);
        RemoveShadowCloak(ent);
    }

    private void OnCloakShutdown(Entity<ShadowCloakEntityComponent> ent, ref ComponentShutdown args)
    {
        var parent = ent.Comp.User ?? Transform(ent).ParentUid;

        if (!RemoveShadowCloak(parent))
            PredictedQueueDel(ent.Owner);
    }

    private void OnGetDoAfterSpeed(Entity<ShadowCloakedComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        if (GetShadowCloakEntity(ent) is { } cloak)
            args.Multiplier *= cloak.Comp.DoAfterSlowdown;
    }

    /// <summary>
    /// Failsafe method in case shadow cloak entity unparents from heretic
    /// </summary>
    private void OnEntParentChanged(Entity<ShadowCloakEntityComponent> ent, ref EntParentChangedMessage args)
    {
        var userIsOldParent = ent.Comp.User == args.OldParent;

        // If we are being deleted, just remove status effect from our old parent
        if (TerminatingOrDeleted(ent))
        {
            RemoveShadowCloak(args.OldParent);
            if (!userIsOldParent)
                RemoveShadowCloak(ent.Comp.User);
            return;
        }

        // If our current parent is shadow cloaked then it's fine - do nothing
        if (_net.IsClient || HasComp<ShadowCloakedComponent>(args.Transform.ParentUid))
            return;

        // Our current parent isn't shadow cloaked - bad
        // If old parent isn't shadow cloaked either or it is being deleted - delete us
        if (TerminatingOrDeleted(args.OldParent) || !HasComp<ShadowCloakedComponent>(args.OldParent))
        {
            PredictedQueueDel(ent.Owner);
            return;
        }

        ent.Comp.User ??= args.OldParent.Value;

        // Parent us to the old user
        _transform.SetParent(ent, args.Transform, ent.Comp.User.Value);
    }

    private void OnShutdown(Entity<ShadowCloakedComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        Shutdown(ent);

        var xform = Transform(ent);

        var revealCooldown = TimeSpan.FromMinutes(1);
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (!_cloakQuery.TryComp(child, out var cloak))
                continue;

            revealCooldown = cloak.RevealCooldown;
            PredictedQueueDel(child);
        }

        _modifier.RefreshMovementSpeedModifiers(ent);

        ResetAbilityCooldown(ent, revealCooldown);
    }

    private void OnStartup(Entity<ShadowCloakedComponent> ent, ref ComponentStartup args)
    {
        Startup(ent);

        _modifier.RefreshMovementSpeedModifiers(ent);

        if (_net.IsClient)
            return;

        var xform = Transform(ent);

        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (!_cloakQuery.TryComp(child, out var shadowCloak))
                continue;

            shadowCloak.User = ent;
            return;
        }

        var cloakEntity = SpawnAttachedTo(ent.Comp.ShadowCloakEntity, ent.Owner.ToCoordinates());
        var cloak = EnsureComp<ShadowCloakEntityComponent>(cloakEntity);
        cloak.User = ent;
        Dirty(cloakEntity, cloak);

        var relay = EnsureComp<TargetInteractionRelayComponent>(cloakEntity);
        relay.RelayEntity = ent;
        Dirty(cloakEntity, relay);

        _appearance.SetData(cloakEntity,
            RotationVisuals.RotationState,
            _standing.IsDown(ent.Owner) ? RotationState.Horizontal : RotationState.Vertical);
    }

    private void ResetAbilityCooldown(EntityUid uid, TimeSpan cooldown)
    {
        var actions = _actions.GetActions(uid);
        foreach (var (actionUid, _) in actions)
        {
            if (_tag.HasTag(actionUid, ActionTag))
                _actions.SetIfBiggerCooldown(actionUid, cooldown);
        }
    }

    public Entity<ShadowCloakEntityComponent>? GetShadowCloakEntity(EntityUid ent)
    {
        var xform = Transform(ent);

        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (!_cloakQuery.TryComp(child, out var shadowCloak))
                continue;

            shadowCloak.User = ent;

            return (child, shadowCloak);
        }

        return null;
    }

    private bool RemoveShadowCloak(EntityUid? ent)
    {
        if (ent == null || TerminatingOrDeleted(ent))
            return false;

        if (!_status.TryEffectsWithComp<HereticCloakedStatusEffectComponent>(ent, out var effects))
        {
            RemCompDeferred<ShadowCloakedComponent>(ent.Value);
            return false;
        }

        var result = false;

        foreach (var effect in effects)
        {
            result = true;
            PredictedQueueDel(effect.Owner);
        }

        return result;
    }

    protected virtual void Startup(Entity<ShadowCloakedComponent> ent) { }

    protected virtual void Shutdown(Entity<ShadowCloakedComponent> ent) { }
}
