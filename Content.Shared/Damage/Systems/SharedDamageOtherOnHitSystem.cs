using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;

namespace Content.Shared.Damage.Systems;

public abstract partial class SharedDamageOtherOnHitSystem : EntitySystem // Trauma - made partial
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DamageExamineSystem _damageExamine = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeTrauma(); // Trauma

        SubscribeLocalEvent<DamageOtherOnHitComponent, DamageExamineEvent>(OnDamageExamine);
        SubscribeLocalEvent<DamageOtherOnHitComponent, AttemptPacifiedThrowEvent>(OnAttemptPacifiedThrow);
    }

    private void OnDamageExamine(Entity<DamageOtherOnHitComponent> ent, ref DamageExamineEvent args)
    {
        _damageExamine.AddDamageExamine(args.Message, _damageable.ApplyUniversalAllModifiers(ent.Comp.Damage * _damageable.UniversalThrownDamageModifier), Loc.GetString("damage-throw"));
    }

    /// <summary>
    /// Prevent players with the Pacified status effect from throwing things that deal damage.
    /// </summary>
    private void OnAttemptPacifiedThrow(Entity<DamageOtherOnHitComponent> ent, ref AttemptPacifiedThrowEvent args)
    {
        args.Cancel("pacified-cannot-throw");
    }
}
