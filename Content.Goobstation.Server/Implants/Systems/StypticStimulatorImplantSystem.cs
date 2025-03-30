using Content.Goobstation.Server.Implants.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class StypticStimulatorImplantSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();
    private readonly Dictionary<EntityUid, Dictionary<string, FixedPoint2>> _originalDamageSpecifiers = new();

    private static readonly string[] AffectedDamageTypes =
    {
        "Heat", "Cold", "Slash", "Blunt", "Piercing", "Poison", "Asphyxiation"
    };

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

        // Store original damage cap if not already stored
        if (!_originalDamageCaps.ContainsKey(user))
            _originalDamageCaps[user] = damageComp.DamageCap;

        // Store original damage specifiers
        var originalSpecifiers = new Dictionary<string, FixedPoint2>();
        foreach (var damage in damageComp.Damage.DamageDict)
        {
            originalSpecifiers[damage.Key] = damage.Value;
        }
        _originalDamageSpecifiers[user] = originalSpecifiers;

        var damageDict = damageComp.Damage.DamageDict;
        damageDict.Clear();
        // Apply new damage modifications
        foreach (var damageType in AffectedDamageTypes)
        {
            damageDict[damageType] = -0.5;
        }
        damageComp.DamageCap = FixedPoint2.Zero;

        // Stop bleeding
        if (TryComp<BloodstreamComponent>(user, out var bloodstream))
            _bloodstreamSystem.TryModifyBleedAmount(user, -10, bloodstream);

        Dirty(user, damageComp);
    }

    private void OnUnimplanted(Entity<StypticStimulatorImplantComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (!TryComp<PassiveDamageComponent>(args.Implanted, out var damageComp))
            return;

        // Restore original damage cap
        if (_originalDamageCaps.TryGetValue(args.Implanted, out var originalCap))
        {
            damageComp.DamageCap = originalCap;
            _originalDamageCaps.Remove(args.Implanted);
        }

        // Restore original damage specifiers
        if (_originalDamageSpecifiers.TryGetValue(args.Implanted, out var originalSpecifiers))
        {
            damageComp.Damage.DamageDict.Clear();
            foreach (var kvp in originalSpecifiers)
            {
                damageComp.Damage.DamageDict[kvp.Key] = kvp.Value;
            }
            _originalDamageSpecifiers.Remove(args.Implanted);
        }
    }
}
