// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Server.Armor;
using Content.Shared.Damage;
using Content.Shared.Flash.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles the Howl ability.
/// Perform a screeching Howl, dazing those nearby and giving you slight damage reduction and speed.
/// The howl will be heard all across the station.
/// </summary>
public sealed class WerewolfHowlSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private  readonly TransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private  readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<StatusEffectsComponent> _statusEffectsQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        _statusEffectsQuery = GetEntityQuery<StatusEffectsComponent>();

        SubscribeLocalEvent<WerewolfHowlComponent, WerewolfHowlEvent>(OnWerewolfHowl);
        SubscribeLocalEvent<WerewolfHowlComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WerewolfHowlComponent>();
        while (query.MoveNext(out var uid, out var howl))
        {
            if (!howl.ApplyEffects)
                continue;

            if (_timing.CurTime < howl.NextUpdate || !howl.Active)
                continue;

            howl.Active = false;
            _movement.RefreshMovementSpeedModifiers(uid);

            // check once more here
            if (howl.NewDamageModifier == null || howl.OldDamageModifier == null)
                return;

            _damageable.SetDamageModifierSetId(uid, howl.OldDamageModifier);
        }
    }

    private void OnWerewolfHowl(Entity<WerewolfHowlComponent> ent, ref WerewolfHowlEvent args)
    {
        PlayVariableSound(ent);

        if (!ent.Comp.ApplyEffects)
            return;

        ApplyStatusEffectsNearby(ent);
        ApplyBuffs(ent);
    }

    private void OnRefreshMovementSpeedModifiers(Entity<WerewolfHowlComponent> ent,
        ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active)
        {
           args.ModifySpeed(ent.Comp.WalkSpeed, ent.Comp.RunSpeed);
        }
        else
        {
            args.ModifySpeed(1f, 1f);
        }
    }

    #region Helpers
    /// <summary>
    ///  Apply the status effects to the nearby entities via lookup
    /// </summary>
    /// <param name="ent"></param> The entity
    private void ApplyStatusEffectsNearby(Entity<WerewolfHowlComponent> ent)
    {
        foreach (var entity in _entityLookup.GetEntitiesInRange(ent.Owner, ent.Comp.Range))
        {
            if (!_statusEffectsQuery.TryComp(entity, out var statusEffects))
                continue;

            // if we are the entity, skip us
            if (entity == ent.Owner)
                continue;

            // apply slowdown
            _statusEffects.TryAddStatusEffect<SlowedDownComponent>(
                entity,
                ent.Comp.SlowedDown,
                ent.Comp.SlowedDownDuration,
                false,
                statusEffects);

            _statusEffects.TryAddStatusEffect<FlashedComponent>(
                entity,
                ent.Comp.Flashed,
                ent.Comp.FlashDuration,
                false,
                statusEffects);
        }
    }

    /// <summary>
    ///  Plays the howl sound for people near the entity
    /// </summary>
    /// <param name="ent"></param>
    private void PlayVariableSound(Entity<WerewolfHowlComponent> ent)
    {
        if (ent.Comp.HowlSoundFar == null || ent.Comp.HowlSoundNear == null)
            return;

        var pos = _transform.GetMapCoordinates(ent.Owner);

        var audioRange = ent.Comp.HowlSoundMaxRange;
        var filter = Filter.Pvs(pos).AddInRange(pos, audioRange);
        var sound = ent.Comp.HowlSoundNear;

        _audio.PlayEntity(sound, filter, ent.Owner, true, sound.Params);

        // yes i copied ts from bombing now sybau
        var farAudioRange = ent.Comp.HowlSoundMaxRange * 4;
        var farFilter = Filter.Empty().AddInRange(pos, farAudioRange).RemoveInRange(pos, audioRange);
        var farSound = ent.Comp.HowlSoundFar;

        _audio.PlayGlobal(farSound, farFilter, true, farSound.Params);
    }

    /// <summary>
    ///  Apply slight damage reduction and speed
    /// </summary>
    /// <param name="ent"></param> The entity
    private void ApplyBuffs(Entity<WerewolfHowlComponent> ent)
    {
        ent.Comp.Active = true;
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.HowlBuffsDuration;
        _movement.RefreshMovementSpeedModifiers(ent.Owner);

        if (ent.Comp.NewDamageModifier == null || ent.Comp.OldDamageModifier == null)
            return;

        _damageable.SetDamageModifierSetId(ent.Owner, ent.Comp.NewDamageModifier);
    }
    #endregion
}
