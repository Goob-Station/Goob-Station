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

    private void UpdateFeared(TimeSpan now)
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
    /// <returns>The victim's component, or null if they have none or belong to another slasher.</returns>
    private SlasherVictimFearBuildupComponent? ObserveVictim(Entity<SlasherFearComponent> slasher, EntityUid other, TimeSpan now)
    {
        var (uid, comp) = slasher;

        if (!_status.HasStatusEffect(other, comp.FearedEffect))
            _status.TryAddStatusEffect(other, comp.FearedEffect, out _, duration: null);

        if (!TryComp<SlasherVictimFearBuildupComponent>(other, out var victim))
            return null;

        victim.SourceEffect = comp.FearedEffect;

        // This stops evil things from happening if there's more than 1 slasher.
        if (victim.Scarer is { } owner
            && owner != uid
            && !TerminatingOrDeleted(owner)
            && HasComp<SlasherFearComponent>(owner)
            && now - victim.LastObserved < victim.OwnershipTimeout)
            return null;

        if (victim.Scarer != uid)
        {
            victim.Scarer = uid;
            if (_net.IsServer)
                ApplyFearStyle((other, victim), comp.FearStyle);
        }

        victim.LastObserved = now;
        return victim;
    }

    private void ReleaseVictims(EntityUid slasher)
    {
        var victims = EntityQueryEnumerator<SlasherVictimFearBuildupComponent>();
        while (victims.MoveNext(out var victimUid, out var victim))
        {
            if (victim.Scarer != slasher)
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
            _movemod.TryUpdateMovementSpeedModDuration(
                uid, comp.SlowEffect, comp.SlowRefresh, comp.SlowMultiplier, comp.SlowMultiplier);

        if (comp.Fear >= comp.DamageThreshold && _net.IsServer)
            _damageable.TryChangeDamage(uid, comp.DamagePerSecond, true);

        Dirty(uid, comp);

        UpdateVictimMusic(ent);
    }

    private void ApplyFearStyle(Entity<SlasherVictimFearBuildupComponent> victim, ComponentRegistry style)
    {
        ClearFearStyle(victim);

        if (style.Count == 0)
            return;

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
