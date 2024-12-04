using Content.Server._Lavaland.Aggression;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Robust.Shared.Player;

namespace Content.Server._Lavaland.Aggression.Systems;

[Virtual]
public partial class AggressorsSystem : EntitySystem
{
    // TODO: make cooldowns for all individual aggressors that fall out of vision range

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AggressiveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<AggressorComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnDamageChanged(Entity<AggressiveComponent> ent, ref DamageChangedEvent args)
    {
        var aggro = args.Origin;

        if (aggro != null && HasComp<ActorComponent>(aggro))
            AddAggressor(ent, (EntityUid) aggro);
    }

    private void OnMobStateChange(Entity<AggressorComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            CleanAggressions(ent);
    }

    #region api

    public List<EntityUid>? GetAggressors(EntityUid uid)
    {
        TryComp<AggressiveComponent>(uid, out var aggro);
        return aggro?.Aggressors ?? null;
    }

    public void RemoveAggressor(Entity<AggressiveComponent> ent, EntityUid aggressor)
    {
        if (ent.Comp.Aggressors.Contains(aggressor))
            ent.Comp.Aggressors.Remove(aggressor);
    }

    public void AddAggressor(Entity<AggressiveComponent> ent, EntityUid aggressor)
    {
        ent.Comp.Aggressors.Add(aggressor);

        var aggcomp = EnsureComp<AggressorComponent>(aggressor);

        if (!aggcomp.Aggressives.Contains(ent))
            aggcomp.Aggressives.Add(ent);
    }

    public void CleanAggressions(EntityUid aggressor)
    {
        if (!TryComp<AggressorComponent>(aggressor, out var aggcomp))
            return;

        foreach (var aggrod in aggcomp.Aggressives)
            if (TryComp<AggressiveComponent>(aggrod, out var aggressors))
                RemoveAggressor((aggrod, aggressors), aggressor);

        RemComp(aggressor, aggcomp);
    }

    #endregion
}
