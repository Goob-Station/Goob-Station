using System.Linq;
using Content.Shared.Actions;
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
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithRevenantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WraithRevenantComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<WraithRevenantComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
    }

    private void OnMapInit(Entity<WraithRevenantComponent> ent, ref MapInitEvent args)
    {
        if (EnsureComp<PassiveDamageComponent>(ent.Owner, out var passiveDamage))
        {
            ent.Comp.HadPassive = true;
            ent.Comp.OldDamageSpecifier = passiveDamage.Damage;
            passiveDamage.Damage = ent.Comp.DamageOvertime;
        }
        else
        {
            passiveDamage.Damage = ent.Comp.DamageOvertime;
            passiveDamage.AllowedStates = ent.Comp.AllowedStates;
        }

        Dirty(ent);
        Dirty(ent.Owner, passiveDamage);

        EntityManager.AddComponents(ent.Owner, _proto.Index(ent.Comp.RevenantAbilities));
    }

    private void OnComponentShutdown(Entity<WraithRevenantComponent> ent, ref ComponentShutdown args)
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

    private void OnBeforeDamageChanged(Entity<WraithRevenantComponent> ent, ref BeforeDamageChangedEvent args)
    {
        // dont let them heal at all
        foreach (var (type, amount) in args.Damage.DamageDict.ToList())
        {
            if (amount < 0)
                args.Damage.DamageDict[type] = 0;
        }
    }
}
