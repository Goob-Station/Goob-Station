using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Mobs;
using Robust.Shared.Player;

namespace Content.Shared._Lavaland.Aggression;

public abstract class SharedAggressorsSystem : EntitySystem
{
    // TODO: make cooldowns for all individual aggressors that fall out of vision range

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AggressiveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<AggressiveComponent, EntityTerminatingEvent>(OnDeleted);
        SubscribeLocalEvent<AggressiveComponent, DestructionEventArgs>(OnDestroyed);

        SubscribeLocalEvent<AggressorComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnDamageChanged(Entity<AggressiveComponent> ent, ref DamageChangedEvent args)
    {
        var aggro = args.Origin;

        if (aggro == null || !HasComp<ActorComponent>(aggro))
            return;

        AddAggressor(ent, aggro.Value);
    }

    private void OnMobStateChange(Entity<AggressorComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            CleanAggressions(ent);
    }

    private void OnDeleted(Entity<AggressiveComponent> ent, ref EntityTerminatingEvent args)
    {
        RemoveAllAggressors(ent);
    }

    private void OnDestroyed(Entity<AggressiveComponent> ent, ref DestructionEventArgs args)
    {
        RemoveAllAggressors(ent);
    }

    #region api

    public HashSet<EntityUid>? GetAggressors(EntityUid uid)
    {
        TryComp<AggressiveComponent>(uid, out var aggro);
        return aggro?.Aggressors ?? null;
    }

    public void RemoveAggressor(Entity<AggressiveComponent> ent, EntityUid aggressor)
    {
        ent.Comp.Aggressors.Remove(aggressor);
        RaiseLocalEvent(ent, new AggressorRemovedEvent(GetNetEntity(aggressor)));
    }

    public void RemoveAllAggressors(Entity<AggressiveComponent> ent)
    {
        var aggressors = ent.Comp.Aggressors;
        ent.Comp.Aggressors.Clear();
        foreach (var aggressor in aggressors)
        {
            RaiseLocalEvent(ent, new AggressorRemovedEvent(GetNetEntity(aggressor)));
        }
    }

    public void AddAggressor(Entity<AggressiveComponent> ent, EntityUid aggressor)
    {
        ent.Comp.Aggressors.Add(aggressor);

        var aggcomp = EnsureComp<AggressorComponent>(aggressor);
        RaiseLocalEvent(ent, new AggressorAddedEvent(GetNetEntity(aggressor)));

        aggcomp.Aggressives.Add(ent);
    }

    public void CleanAggressions(EntityUid aggressor)
    {
        if (!TryComp<AggressorComponent>(aggressor, out var aggcomp))
            return;

        foreach (var aggrod in aggcomp.Aggressives)
        {
            if (TryComp<AggressiveComponent>(aggrod, out var aggressors))
                RemoveAggressor((aggrod, aggressors), aggressor);
        }

        RemComp(aggressor, aggcomp);
    }

    #endregion
}
