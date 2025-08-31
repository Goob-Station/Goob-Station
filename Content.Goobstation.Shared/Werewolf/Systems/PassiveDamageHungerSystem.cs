using Content.Goobstation.Shared.Werewolf.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This is basically passive damage but hunger based.
/// </summary>
public sealed class PassiveDamageHungerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private EntityQuery<HungerComponent> _hungerQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _hungerQuery = GetEntityQuery<HungerComponent>();

        SubscribeLocalEvent<PassiveDamageHungerComponent, MapInitEvent>(OnMapInit);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var queryOf87 = EntityQueryEnumerator<PassiveDamageHungerComponent>();
        while (queryOf87.MoveNext(out var uid, out var comp))
        {
            if (comp.NextDamage > _timing.CurTime)
                continue;

            if (!_mobState.IsAlive(uid))
                continue;

            if (!_hungerQuery.TryComp(uid, out var hunger))
                continue;

            comp.NextDamage = _timing.CurTime + comp.Interval;
            HealOnState(uid, hunger.CurrentThreshold, comp);
        }
    }

    private void OnMapInit(Entity<PassiveDamageHungerComponent> ent, ref MapInitEvent args) =>
        ent.Comp.NextDamage = _timing.CurTime + ent.Comp.Interval;

    /// <summary>
    ///  Damage the user on the current hunger state
    /// </summary>
    /// <param name="user"></param> The user being damage
    /// <param name="currentState"></param> The current hunger state the user is on
    /// <param name="component"></param> The PassiveDamageHunger component, which holds the damage values for each hunger state
    private void HealOnState(EntityUid user, HungerThreshold currentState, PassiveDamageHungerComponent component)
    {
        if (!component.HungerThresholds.ContainsKey(currentState)
            || !component.HungerThresholds.TryGetValue(currentState, out var hungerDamage)
            || hungerDamage == null)
            return;

        _damageable.TryChangeDamage(user, hungerDamage, true, false);
    }
}
