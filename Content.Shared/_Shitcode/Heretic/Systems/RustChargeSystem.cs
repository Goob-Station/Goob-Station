// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class RustChargeSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RustChargeComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<RustChargeComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<RustChargeComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<RustChargeComponent, StopThrowEvent>(OnStopThrow);
        SubscribeLocalEvent<RustChargeComponent, DownAttemptEvent>(OnDownAttempt);
        SubscribeLocalEvent<RustChargeComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<RustChargeComponent, BeforeStatusEffectAddedEvent>(OnBeforeRustChargeStatusEffect);
        SubscribeLocalEvent<RustChargeComponent, ComponentShutdown>(OnRustChargeShutdown);
    }

    private void OnRustChargeShutdown(Entity<RustChargeComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        RemCompDeferred<RustObjectsInRadiusComponent>(ent);
    }

    private void OnBeforeRustChargeStatusEffect(Entity<RustChargeComponent> ent, ref BeforeStatusEffectAddedEvent args)
    {
        if (args.Key == "KnockedDown")
            args.Cancelled = true;
    }

    private void OnInteractAttempt(Entity<RustChargeComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDownAttempt(Entity<RustChargeComponent> ent, ref DownAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnPreventCollide(Entity<RustChargeComponent> ent, ref PreventCollideEvent args)
    {
        if (!args.OtherFixture.Hard)
            return;

        var other = args.OtherEntity;

        if (!HasComp<DamageableComponent>(other) || _tag.HasTag(other, "IgnoreImmovableRod") ||
            ent.Comp.DamagedEntities.Contains(other))
            args.Cancelled = true;
    }

    private void OnStopThrow(Entity<RustChargeComponent> ent, ref StopThrowEvent args)
    {
        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnLand(Entity<RustChargeComponent> ent, ref LandEvent args)
    {
        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnCollide(Entity<RustChargeComponent> ent, ref StartCollideEvent args)
    {
        if (!args.OtherFixture.Hard)
            return;

        var other = args.OtherEntity;

        if (ent.Comp.DamagedEntities.Contains(other))
            return;

        _audio.PlayPredicted(ent.Comp.HitSound, ent, ent);

        ent.Comp.DamagedEntities.Add(other);

        // I would check for DamageableComponent but it is in server for whatever reason, also prevent collide handles that
        if (!TryComp(other, out DamageableComponent? damageable) || _tag.HasTag(other, "IgnoreImmovableRod"))
            return;

        // Damage mobs
        if (HasComp<MobStateComponent>(other))
        {
            _stun.KnockdownOrStun(other, ent.Comp.KnockdownTime, true);

            _damageable.TryChangeDamage(other,
                ent.Comp.Damage,
                false,
                true,
                damageable,
                targetPart: TargetBodyPart.Torso);

            return;
        }

        // Delete structures
        if (_net.IsServer)
            Del(other);
    }
}
