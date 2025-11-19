using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Revenant;

/// <summary>
/// This handles the revenant system for wraith.
/// Just adds the abilities and passive damage shittery
/// </summary>
public sealed class WraithRevenantSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithRevenantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WraithRevenantComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<WraithRevenantComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<WraithRevenantComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
    }

    private void OnMapInit(Entity<WraithRevenantComponent> ent, ref MapInitEvent args) =>
        EntityManager.AddComponents(ent.Owner, _proto.Index(ent.Comp.RevenantAbilities));

    private void OnMobStateChanged(Entity<WraithRevenantComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            RemComp<WraithRevenantComponent>(ent);
    }

    private void OnComponentShutdown(Entity<WraithRevenantComponent> ent, ref ComponentShutdown args) =>
        Reset(ent);

    private void OnBeforeDamageChanged(Entity<WraithRevenantComponent> ent, ref BeforeDamageChangedEvent args)
    {
        // dont let them heal at all
        foreach (var (type, amount) in args.Damage.DamageDict.ToList())
        {
            if (amount < 0)
                args.Damage.DamageDict[type] = 0;
        }
    }

    #region Helpers
    private void Reset(Entity<WraithRevenantComponent> ent)
    {
        EntityManager.RemoveComponents(ent.Owner, _proto.Index(ent.Comp.RevenantAbilities));

        if (!TryComp<PassiveDamageComponent>(ent.Owner, out var comp)
            || ent.Comp.OldDamageSpecifier == null)
            return;

        if (ent.Comp.HadPassive)
        {
            comp.Damage = ent.Comp.OldDamageSpecifier;
            return;
        }

        RemComp<PassiveDamageComponent>(ent.Owner);
    }

    public void SetPassiveDamageValues(Entity<WraithRevenantComponent> ent, DamageSpecifier newDamage, List<MobState> allowedStates)
    {
        if (TryComp<PassiveDamageComponent>(ent.Owner, out var passive))
        {
            ent.Comp.HadPassive = true;
            ent.Comp.OldDamageSpecifier = passive.Damage;
            passive.Damage = newDamage;
            Dirty(ent.Owner, passive);
        }
        else
        {
            var passiveDamage = EnsureComp<PassiveDamageComponent>(ent.Owner);
            passiveDamage.Damage = newDamage;
            passiveDamage.AllowedStates = allowedStates;
            Dirty(ent.Owner, passiveDamage);
        }

        Dirty(ent);
    }
    #endregion
}
