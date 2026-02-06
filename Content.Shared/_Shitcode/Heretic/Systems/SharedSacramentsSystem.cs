using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedSacramentsSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedStaminaSystem _stam = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SacramentsOfPowerComponent, BeforeDamageChangedEvent>(OnBeforeDamageChange);
        SubscribeLocalEvent<SacramentsOfPowerComponent, BeforeStaminaDamageEvent>(OnBeforeStamina);
    }

    private void OnBeforeStamina(Entity<SacramentsOfPowerComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (ent.Comp.State != SacramentsState.Open || args.Value <= 0f || args.Source == ent.Owner)
            return;

        args.Cancelled = true;
        Pulse(ent);

        if (args.Source == null || HasComp<SacramentsOfPowerComponent>(args.Source.Value))
            return;

        _stam.TakeStaminaDamage(args.Source.Value, args.Value, applyResistances: true, source: ent);
    }

    private void OnBeforeDamageChange(Entity<SacramentsOfPowerComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (ent.Comp.State != SacramentsState.Open || !args.Damage.AnyPositive() || args.Origin == ent.Owner)
            return;

        args.Cancelled = true;
        Pulse(ent);

        if (args.Origin == null || HasComp<SacramentsOfPowerComponent>(args.Origin.Value))
            return;

        _dmg.TryChangeDamage(args.Origin.Value,
            args.Damage * ent.Comp.DamageReturnRatio,
            targetPart: TargetBodyPart.Vital,
            origin: ent,
            canMiss: false);
    }

    protected virtual void Pulse(EntityUid ent) { }
}

[Serializable, NetSerializable]
public sealed class SacramentsPulseEvent(NetEntity entity) : EntityEventArgs
{
    public NetEntity Entity = entity;
}

[Serializable, NetSerializable]
public enum SacramentsKey
{
    Key
}

[Serializable, NetSerializable]
public enum SacramentsState
{
    Opening,
    Open,
    Closing
}
