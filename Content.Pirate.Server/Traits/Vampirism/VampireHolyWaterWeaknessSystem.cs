using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Religion;
using Content.Pirate.Server.Traits.Vampirism.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.Traits.Vampirism;

/// <summary>
///     Makes any vampire (trait or antagonist) carrying a <see cref="VampireHolyWaterWeaknessComponent"/>
///     vulnerable to holy damage and burn from holy water, so both types suffer identically.
/// </summary>
public sealed class VampireHolyWaterWeaknessSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private static readonly ProtoId<DamageGroupPrototype> _burnGroupId = "Burn";

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<VampireHolyWaterWeaknessComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (TryComp(uid, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
                continue;

            HandleHolyWater((uid, comp));
        }
    }

    private void HandleHolyWater(Entity<VampireHolyWaterWeaknessComponent> ent)
    {
        var comp = ent.Comp;
        if (_timing.CurTime < comp.NextHolyWaterTick)
            return;

        var holywater = _solution.GetTotalPrototypeQuantity(ent, comp.HolyWaterReagentId);
        if (holywater <= FixedPoint2.Zero)
            return;

        comp.NextHolyWaterTick = _timing.CurTime + comp.HolyTickDelay;

        if (_proto.TryIndex<DamageGroupPrototype>(_burnGroupId, out var burn))
            _damageable.TryChangeDamage(ent, new DamageSpecifier(burn, FixedPoint2.New(comp.HolyWaterBurnDamage)), true);

        if (_rand.Prob(comp.HolyWaterFireChance))
            _flammable.AdjustFireStacks(ent, comp.HolyWaterFireStacks, ignite: true);
    }
}
