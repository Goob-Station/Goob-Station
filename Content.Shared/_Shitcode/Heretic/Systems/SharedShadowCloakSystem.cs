using Content.Goobstation.Common.Identity;
using Content.Goobstation.Common.Projectiles;
using Content.Goobstation.Common.Speech;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Systems;
using Content.Shared.Rotation;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedShadowCloakSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _modifier = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private static readonly ProtoId<TagPrototype> ActionTag = "ShadowCloakAction";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCloakedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowCloakedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ShadowCloakedComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<ShadowCloakedComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<ShadowCloakedComponent, GetDoAfterDelayMultiplierEvent>(OnGetDoAfterSpeed);
        SubscribeLocalEvent<ShadowCloakedComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<ShadowCloakedComponent, TransformSpeakerNameEvent>(OnTransformName);
        SubscribeLocalEvent<ShadowCloakedComponent, TryGetIdentityShortInfoEvent>(OnGetIdentity);
        SubscribeLocalEvent<ShadowCloakedComponent, GetIdentityRepresentationEntityEvent>(OnGetIdentityEntity);
        SubscribeLocalEvent<ShadowCloakedComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<ShadowCloakedComponent, StoodEvent>(OnStood);
        SubscribeLocalEvent<ShadowCloakedComponent, ShouldTargetedProjectileCollideEvent>(OnShouldTarget);
        SubscribeLocalEvent<ShadowCloakedComponent, GetSpeechSoundEvent>(OnGetSpeechSound);
        SubscribeLocalEvent<ShadowCloakedComponent, GetEmoteSoundsEvent>(OnGetEmoteSound);

        SubscribeLocalEvent<ShadowCloakEntityComponent, EntParentChangedMessage>(OnEntParentChanged);
        SubscribeLocalEvent<ShadowCloakEntityComponent, DamageChangedEvent>(OnCloakDamaged);
        SubscribeLocalEvent<ShadowCloakEntityComponent, ComponentShutdown>(OnCloakShutdown);
    }

    private void OnShouldTarget(Entity<ShadowCloakedComponent> ent, ref ShouldTargetedProjectileCollideEvent args)
    {
        if (args.Target == GetShadowCloakEntity(ent))
            args.Handled = true;
    }

    private void OnGetEmoteSound(Entity<ShadowCloakedComponent> ent, ref GetEmoteSoundsEvent args)
    {
        var uid = GetShadowCloakEntity(ent);
        if (uid != null)
            args.EmoteSoundProtoId = ent.Comp.EmoteSounds;
    }

    private void OnGetSpeechSound(Entity<ShadowCloakedComponent> ent, ref GetSpeechSoundEvent args)
    {
        var uid = GetShadowCloakEntity(ent);
        if (uid != null)
            args.SpeechSoundProtoId = ent.Comp.SpeechSounds;
    }

    private void OnStood(Entity<ShadowCloakedComponent> ent, ref StoodEvent args)
    {
        var uid = GetShadowCloakEntity(ent);
        if (uid != null)
            _appearance.SetData(uid.Value, RotationVisuals.RotationState, RotationState.Vertical);
    }

    private void OnDowned(Entity<ShadowCloakedComponent> ent, ref DownedEvent args)
    {
        var uid = GetShadowCloakEntity(ent);
        if (uid != null)
            _appearance.SetData(uid.Value, RotationVisuals.RotationState, RotationState.Horizontal);
    }

    private void OnGetIdentityEntity(Entity<ShadowCloakedComponent> ent, ref GetIdentityRepresentationEntityEvent args)
    {
        var cloak = GetShadowCloakEntity(ent);
        if (cloak != null)
            args.Uid = cloak.Value;
    }

    private void OnGetIdentity(Entity<ShadowCloakedComponent> ent, ref TryGetIdentityShortInfoEvent args)
    {
        var cloak = GetShadowCloakEntity(ent);
        if (cloak != null)
            args.Title = Name(cloak.Value);
    }

    private void OnTransformName(Entity<ShadowCloakedComponent> ent, ref TransformSpeakerNameEvent args)
    {
        args.SpeechVerb = ent.Comp.SpeechVerb;
        var cloak = GetShadowCloakEntity(ent);
        if (cloak != null)
            args.VoiceName = Name(cloak.Value);
    }

    private void OnDamageChanged(Entity<ShadowCloakedComponent> ent, ref DamageChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        ent.Comp.SustainedDamage += args.DamageDelta.GetTotal();

        if (ent.Comp.SustainedDamage < ent.Comp.DamageBeforeReveal)
            return;

        if (!_random.Prob(Math.Clamp(ent.Comp.SustainedDamage.Float() / 100f, 0f, 1f)))
            return;

        if (ent.Comp.DebuffOnEarlyReveal)
        {
            _stun.KnockdownOrStun(ent, ent.Comp.KnockdownTime, true);
            var (walk, sprint) = ent.Comp.EarlyRemoveMoveSpeedModifiers;
            _stun.TrySlowdown(ent, ent.Comp.SlowdownTime, true, walk, sprint);
        }

        ResetAbilityCooldown(ent, ent.Comp.ForceRevealCooldown);
        RemoveShadowCloak(ent);
    }

    private void OnCloakShutdown(Entity<ShadowCloakEntityComponent> ent, ref ComponentShutdown args)
    {
        var parent = Transform(ent).ParentUid;

        if (!TerminatingOrDeleted(parent) && TryComp(parent, out ShadowCloakedComponent? shadowCloaked))
            RemoveShadowCloak((parent, shadowCloaked));
        else if (_net.IsServer && !TerminatingOrDeleted(ent))
            QueueDel(ent);
    }

    private void OnGetDoAfterSpeed(Entity<ShadowCloakedComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.DoAfterSlowdown;
    }

    private void OnRefreshMoveSpeed(Entity<ShadowCloakedComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var (walk, sprint) = ent.Comp.MoveSpeedModifiers;
        args.ModifySpeed(walk, sprint);
    }

    private void OnCloakDamaged(Entity<ShadowCloakEntityComponent> ent, ref DamageChangedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var xform = Transform(ent);

        if (TerminatingOrDeleted(xform.ParentUid) || !HasComp<ShadowCloakedComponent>(xform.ParentUid))
        {
            AttemptDeleteShadowCloakEntity(ent);
            return;
        }

        if (args.DamageDelta is not { } damage)
            return;

        _damageable.TryChangeDamage(xform.ParentUid,
            damage,
            origin: args.Origin,
            interruptsDoAfters: args.InterruptsDoAfters);
    }

    private void OnEntParentChanged(Entity<ShadowCloakEntityComponent> ent, ref EntParentChangedMessage args)
    {
        if (TerminatingOrDeleted(ent) || ent.Comp.DeletionAccumulator != null)
        {
            if (args.OldParent != null && !TerminatingOrDeleted(args.OldParent.Value) &&
                TryComp(args.OldParent.Value, out ShadowCloakedComponent? shadowCloaked))
                RemoveShadowCloak((args.OldParent.Value, shadowCloaked));
            return;
        }

        if (_net.IsClient || HasComp<ShadowCloakedComponent>(args.Transform.ParentUid))
            return;

        if (TerminatingOrDeleted(args.OldParent) || !HasComp<ShadowCloakedComponent>(args.OldParent))
        {
            AttemptDeleteShadowCloakEntity(ent);
            return;
        }

        _transform.SetParent(ent, args.Transform, args.OldParent.Value);
    }

    private void OnRemove(Entity<ShadowCloakedComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _modifier.RefreshMovementSpeedModifiers(ent);

        ResetAbilityCooldown(ent, ent.Comp.RevealCooldown);
    }

    private void OnShutdown(Entity<ShadowCloakedComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        Shutdown(ent);

        var xform = Transform(ent);

        var shadowCloakQuery = GetEntityQuery<ShadowCloakEntityComponent>();
        var children = xform.ChildEnumerator;
        List<Entity<ShadowCloakEntityComponent>> toDelete = new();
        while (children.MoveNext(out var child))
        {
            if (shadowCloakQuery.TryComp(child, out var comp))
                toDelete.Add((child, comp));
        }

        foreach (var child in toDelete)
        {
            AttemptDeleteShadowCloakEntity(child);
        }

        if (_net.IsClient)
            return;

        _audio.PlayPvs(ent.Comp.Sound, ent);
    }

    private void OnStartup(Entity<ShadowCloakedComponent> ent, ref ComponentStartup args)
    {
        Startup(ent);

        _modifier.RefreshMovementSpeedModifiers(ent);

        if (_net.IsClient)
            return;

        _audio.PlayPvs(ent.Comp.Sound, ent);

        var xform = Transform(ent);

        var shadowCloakQuery = GetEntityQuery<ShadowCloakEntityComponent>();
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (shadowCloakQuery.HasComponent(child))
                return;
        }

        var cloakEntity = SpawnAttachedTo(ent.Comp.ShadowCloakEntity, ent.Owner.ToCoordinates());
        EnsureComp<ShadowCloakEntityComponent>(cloakEntity);

        _appearance.SetData(cloakEntity,
            RotationVisuals.RotationState,
            _standing.IsDown(ent) ? RotationState.Horizontal : RotationState.Vertical);
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

    private EntityUid? GetShadowCloakEntity(EntityUid ent)
    {
        var xform = Transform(ent);

        var shadowCloakQuery = GetEntityQuery<ShadowCloakEntityComponent>();
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (!shadowCloakQuery.HasComponent(child))
                continue;

            return child;
        }

        return null;
    }

    private void AttemptDeleteShadowCloakEntity(Entity<ShadowCloakEntityComponent> ent)
    {
        ent.Comp.DeletionAccumulator ??= ent.Comp.Lifetime;

        var xform = Transform(ent);
        if (!xform.ParentUid.IsValid())
            return;

        _transform.DetachEntity(ent, xform);
    }

    private void RemoveShadowCloak(Entity<ShadowCloakedComponent> ent)
    {
        _status.TryRemoveStatusEffect(ent, ent.Comp.Status, remComp: false);
        RemCompDeferred(ent.Owner, ent.Comp);
    }

    protected virtual void Startup(Entity<ShadowCloakedComponent> ent) { }

    protected virtual void Shutdown(Entity<ShadowCloakedComponent> ent) { }
}
