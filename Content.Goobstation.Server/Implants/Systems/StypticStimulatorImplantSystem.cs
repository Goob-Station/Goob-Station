using Content.Goobstation.Server.Implants.Components;
using Content.Server.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class StypticStimulatorImplantSystem : EntitySystem
{

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();
    private Dictionary<String, FixedPoint2> _originalDamageSpecifier = new();

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

        // Store original damage cap.
        if (!_originalDamageCaps.ContainsKey(user))
            _originalDamageCaps[user] = damageComp.DamageCap;

        // Foreach passive damage specifier we have, save it to a dictionary.
        foreach (var damage in damageComp.Damage.DamageDict)
        {
            _originalDamageSpecifier.Add(damage.Key, damage.Value);
        }

        // This is kind of a shit way to do this but... it works!
        // Clear current dictionary
        damageComp.Damage.DamageDict.Clear();
        // Add our damage types.
        damageComp.Damage.DamageDict.Add("Heat", -0.5);
        damageComp.Damage.DamageDict.Add("Cold", -0.5);
        damageComp.Damage.DamageDict.Add("Slash", -0.5);
        damageComp.Damage.DamageDict.Add("Blunt", -0.5);
        damageComp.Damage.DamageDict.Add("Piercing", -0.5);
        damageComp.Damage.DamageDict.Add("Poison", -0.5);
        // Set the damage cap to zero
        damageComp.DamageCap = FixedPoint2.Zero;

        // Stop any bleeding
        if (TryComp<BloodstreamComponent>(user, out var bloodstream))
            bloodstream.BleedAmount = 0;

        DirtyEntity(user);

    }

    private void OnUnimplanted(Entity<StypticStimulatorImplantComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (TryComp<PassiveDamageComponent>(args.Implanted, out var damageComp))
        {
            if (_originalDamageCaps.TryGetValue(args.Implanted, out var originalCap))
            {
                // Set the damage cap to the original.
                damageComp.DamageCap = originalCap;
                _originalDamageCaps.Remove(args.Implanted);
            }
            // Clear the current damage dictionary, and set it to the original.
            damageComp.Damage.DamageDict.Clear();
            damageComp.Damage.DamageDict = _originalDamageSpecifier;
        }

        if (HasComp<StypticStimulatorImplantComponent>(args.Implant))
            RemComp<StypticStimulatorImplantComponent>(ent);
    }
}
