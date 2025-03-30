using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class StypticStimulatorImplantSystem : EntitySystem
{

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();
    private DamageSpecifier _originalDamageSpecifier = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }

    private void OnImplant(Entity<StypticStimulatorImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        var user = args.Implanted.Value;

        var damageComp = EnsureComp<PassiveDamageComponent>(user);

        if (!_originalDamageCaps.ContainsKey(user))
            _originalDamageCaps[user] = damageComp.DamageCap;
        _originalDamageSpecifier = damageComp.Damage;

        // This is kind of a shit way to do this but... it works!
        damageComp.Damage.DamageDict.Clear();
        damageComp.Damage.DamageDict.Add("Heat", -0.5);
        damageComp.Damage.DamageDict.Add("Cold", -0.5);
        damageComp.Damage.DamageDict.Add("Slash", -0.5);
        damageComp.Damage.DamageDict.Add("Blunt", -0.5);
        damageComp.Damage.DamageDict.Add("Piercing", -0.5);
        damageComp.Damage.DamageDict.Add("Poison", -0.5);
        damageComp.DamageCap = FixedPoint2.Zero;
        DirtyEntity(user);

    }

    private void OnUnimplanted(Entity<StypticStimulatorImplantComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (TryComp<PassiveDamageComponent>(args.Implanted, out var damageComp))
        {
            if (_originalDamageCaps.TryGetValue(args.Implanted, out var originalCap))
            {
                damageComp.DamageCap = originalCap;
                _originalDamageCaps.Remove(args.Implanted);

                damageComp.Damage = _originalDamageSpecifier;
            }
        }

        if (HasComp<StypticStimulatorImplantComponent>(args.Implant))
            RemComp<StypticStimulatorImplantComponent>(ent);
    }
}
