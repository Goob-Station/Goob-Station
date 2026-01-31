using System.Linq;
using Content.Shared._Goobstation.Wizard.SanguineStrike;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared.Heretic.Effects;

public sealed class WoundedSoldierSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedSanguineStrikeSystem _lifeSteal = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    private const float DamageInterval = 1f;
    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WoundedSoldierComponent, MeleeAttackEvent>(OnAttack);
        SubscribeLocalEvent<WoundedSoldierComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);

        SubscribeLocalEvent<MeleeHitEvent>(OnHit);
    }

    private void OnBeforeDamageChanged(Entity<WoundedSoldierComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (!args.Damage.AnyPositive())
            return;

        if (!_mobState.IsAlive(ent.Owner))
            return;

        if (!_threshold.TryGetThresholdForState(ent.Owner, MobState.Critical, out var threshold))
            return;

        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return;

        if (_threshold.CheckVitalDamage(ent, damageable) + args.Damage.GetTotal() < threshold)
            return;

        args.Cancelled = true;
    }

    private void OnHit(MeleeHitEvent args)
    {
        var user = args.User;

        if (!TryComp(user, out WoundedSoldierComponent? soldier))
            return;

        var hitCount = args.HitEntities.Count(x => x != user && !_mobState.IsDead(x));

        if (hitCount == 0)
            return;

        var total = args.BaseDamage.GetTotal() * hitCount;

        _lifeSteal.LifeSteal(user, total * soldier.LifeStealMultiplier);
        _stamina.TryTakeStamina(user, -total.Float() * soldier.StaminaHealMultiplier);
    }

    private void OnAttack(Entity<WoundedSoldierComponent> ent, ref MeleeAttackEvent args)
    {
        if (!TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon) || !TryComp(ent, out DamageableComponent? dmg))
            return;

        var rate = weapon.NextAttack - _timing.CurTime;
        weapon.NextAttack -= rate - rate / (MathF.Pow(dmg.TotalDamage.Float() * 0.1f, 0.5f) + 1f);
        Dirty(args.Weapon, weapon);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        _accumulator += frameTime;

        if (_accumulator < DamageInterval)
            return;

        _accumulator = 0f;

        var query = EntityQueryEnumerator<WoundedSoldierComponent, MobStateComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var soldier, out var state, out var dmg))
        {
            if (state.CurrentState != MobState.Alive)
                continue;

            _dmg.TryChangeDamage(uid, soldier.DamageOverTime, true, false, dmg, targetPart: TargetBodyPart.Vital);
        }
    }
}
