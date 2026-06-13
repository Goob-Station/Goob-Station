using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
/// Causes the zombie infection on this entity.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class CauseZombieInfectionEntityEffectsSystem : EntityEffectSystem<MobStateComponent, CauseZombieInfection>
{
    // MobState because you have to die to become a zombie...
    protected override void Effect(Entity<MobStateComponent> entity, ref EntityEffectEvent<CauseZombieInfection> args)
    {
        if (HasComp<ZombieImmuneComponent>(entity) || HasComp<IncurableZombieComponent>(entity))
            return;

        EnsureComp<ZombifyOnDeathComponent>(entity);
        EnsureComp<PendingZombieComponent>(entity);
    }
}

/// <summary>
/// Cures the Zombie infection on this entity and optionally inoculates them against future infection.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class CureZombieInfectionEntityEffectsSystem : EntityEffectSystem<MobStateComponent, CureZombieInfection>
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    // MobState because you have to die to become a zombie...
    protected override void Effect(Entity<MobStateComponent> entity, ref EntityEffectEvent<CureZombieInfection> args)
    {
        if (HasComp<IncurableZombieComponent>(entity))
            return;

        // Goob start cure
        if (HasComp<ZombifyOnDeathComponent>(entity)
            || HasComp<PendingZombieComponent>(entity))
        {
            RemComp<ZombifyOnDeathComponent>(entity);
            RemComp<PendingZombieComponent>(entity);

            _popup.PopupEntity(
                Loc.GetString("zombie-cured-popup"),
                entity,
                PopupType.Medium
            );
        }
        // Goob end cure

        if (args.Effect.Innoculate)
            EnsureComp<ZombieImmuneComponent>(entity);

        // Goob cure start, again,
        if (HasComp<ZombieComponent>(entity)
            && entity.Comp.CurrentState != MobState.Alive)
        {
            var ev = new EntityZombifiedEvent(entity);
            RaiseLocalEvent(entity, ref ev);
        }
        // Goob cure end, again.
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class CauseZombieInfection : EntityEffectBase<CauseZombieInfection>
{
    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-cause-zombie-infection", ("chance", Probability));
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class CureZombieInfection : EntityEffectBase<CureZombieInfection>
{
    /// <summary>
    /// Do we also protect against future infections?
    /// </summary>
    [DataField]
    public bool Innoculate;

    /// <summary>
    ///  Goobstation - whether it cures zombies in a critical state or under
    /// </summary>
    [DataField]
    public bool CureCriticalZombies; // Goob

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (Innoculate)
            return Loc.GetString("entity-effect-guidebook-innoculate-zombie-infection", ("chance", Probability));

        return Loc.GetString("entity-effect-guidebook-cure-zombie-infection", ("chance", Probability));
    }
}
