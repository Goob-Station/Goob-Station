using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems.Abilities;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class HealingAuraSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;

    [Dependency] private readonly SharedHereticAbilitySystem _heretic = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<HealingAuraComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var aura, out var xform))
        {
            aura.Accumulator += frameTime;

            if (aura.Accumulator < aura.HealDelay)
                continue;

            aura.Accumulator = 0f;

            var lookup = _lookup.GetEntitiesInRange<DamageableComponent>(xform.Coordinates, aura.Range);
            foreach (var (ent, damageable) in lookup)
            {
                var multiplier = GetHealMultiplier(ent, (uid, aura));
                if (multiplier == 0f)
                    continue;

                _heretic.IHateWoundMed((ent, damageable, null, null),
                    aura.ToHeal * multiplier,
                    aura.BoneHeal * multiplier,
                    aura.PainHeal * multiplier,
                    aura.WoundHeal * multiplier);
            }
        }
    }

    private float GetHealMultiplier(EntityUid toHeal, Entity<HealingAuraComponent> ent)
    {
        var (uid, aura) = ent;

        if (uid == toHeal)
            return aura.SelfHealMultiplier;

        if (_whitelist.IsWhitelistFail(aura.Whitelist, toHeal))
            return 0f;

        if (aura.ComponentHealMultipliers == null)
            return 1f;

        var multiplier = 0f;
        foreach (var (key, value) in aura.ComponentHealMultipliers)
        {
            if (!_compFact.TryGetRegistration(key, out var reg))
            {
                Log.Error($"Unknown component: ${key}");
                aura.ComponentHealMultipliers.Remove(key);
                return 0f;
            }

            if (!HasComp(toHeal, reg.Type))
                continue;

            var sign = multiplier == 0 ? 1 : MathF.Sign(multiplier);
            multiplier = sign * MathF.Max(MathF.Abs(multiplier), MathF.Abs(value));
        }

        return multiplier;
    }
}
