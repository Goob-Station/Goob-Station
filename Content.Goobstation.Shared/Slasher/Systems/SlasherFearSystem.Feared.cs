using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// The victim side of the fear system.
/// </summary>
public sealed partial class SlasherFearSystem
{
    private void OnFearedDetached(Entity<SlasherVictimFearBuildupComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        FadeVictimMusic(ent);
    }

    private void OnFearedShutdown(Entity<SlasherVictimFearBuildupComponent> ent, ref ComponentShutdown args)
    {
        FadeVictimMusic(ent);

        if (_net.IsServer)
            ClearFearStyle(ent);
    }

    private void UpdateVictimFearBuildup(TimeSpan now)
    {
        var feared = EntityQueryEnumerator<SlasherVictimFearBuildupComponent>();
        while (feared.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextUpdate)
                continue;

            comp.NextUpdate = now + comp.UpdateInterval;
            UpdateVictim((uid, comp), now);
        }
    }

    /// <summary>
    /// Marks a victim as seen by a slasher.
    /// </summary>
    /// <returns>False if the victim has no fear buildup component or belongs to another slasher.</returns>
    private bool TryObserveVictim(Entity<SlasherFearComponent> slasher,
        EntityUid other,
        TimeSpan now,
        [NotNullWhen(true)] out SlasherVictimFearBuildupComponent? victim)
    {
        var (uid, comp) = slasher;
        victim = null;

        if (!_status.HasStatusEffect(other, comp.FearedEffect))
            _status.TryAddStatusEffect(other, comp.FearedEffect, out _, duration: null);

        if (!TryComp<SlasherVictimFearBuildupComponent>(other, out var buildup))
            return false;

        buildup.SourceEffect = comp.FearedEffect;

        var netUid = GetNetEntity(uid);

        // This stops evil things from happening if there's more than 1 slasher.
        if (GetEntity(buildup.Scarer) is { } owner
            && owner != uid
            && !TerminatingOrDeleted(owner)
            && HasComp<SlasherFearComponent>(owner)
            && now - buildup.LastObserved < buildup.OwnershipTimeout)
            return false;

        if (buildup.Scarer != netUid)
        {
            buildup.Scarer = netUid;
            if (_net.IsServer)
                ApplyFearStyle((other, buildup), comp.GrantedToVictimOnSight);
        }

        buildup.LastObserved = now;
        victim = buildup;
        return true;
    }

    private void ReleaseVictims(EntityUid slasher)
    {
        var netSlasher = GetNetEntity(slasher);
        var victims = EntityQueryEnumerator<SlasherVictimFearBuildupComponent>();
        while (victims.MoveNext(out var victimUid, out var victim))
        {
            if (victim.Scarer != netSlasher)
                continue;

            victim.Scarer = null;
            Dirty(victimUid, victim);
        }
    }

    private void UpdateVictim(Entity<SlasherVictimFearBuildupComponent> ent, TimeSpan now)
    {
        var (uid, comp) = ent;

        var sinceObserved = now - comp.LastObserved;
        var observed = sinceObserved < comp.ObserveTimeout;

        if (observed)
            comp.Fear = MathF.Min(1f, comp.Fear + comp.GainPerSecond);
        else if (sinceObserved >= comp.FearGracePeriod)
            comp.Fear = MathF.Max(0f, comp.Fear - comp.DecayPerSecond);

        if (comp.Fear <= 0f && !observed)
        {
            _status.TryRemoveStatusEffect(uid, comp.SourceEffect);
            return;
        }

        if (comp.Fear >= comp.SlowThreshold)
            _movementMod.TryUpdateMovementSpeedModDuration(
                uid, comp.SlowEffect, comp.SlowRefresh, comp.SlowMultiplier, comp.SlowMultiplier);

        if (comp.Fear >= comp.DamageThreshold && _net.IsServer)
            _damageable.TryChangeDamage(uid, comp.DamagePerSecond, true);

        Dirty(uid, comp);
        UpdateVictimMusic(ent);
    }

    private void ApplyFearStyle(Entity<SlasherVictimFearBuildupComponent> victim, ComponentRegistry style)
    {
        ClearFearStyle(victim);

        EntityManager.AddComponents(victim.Owner, style);
        victim.Comp.AppliedStyle = style;
    }

    private void ClearFearStyle(Entity<SlasherVictimFearBuildupComponent> victim)
    {
        if (victim.Comp.AppliedStyle is not { } style)
            return;

        EntityManager.RemoveComponents(victim.Owner, style);
        victim.Comp.AppliedStyle = null;
    }
}
