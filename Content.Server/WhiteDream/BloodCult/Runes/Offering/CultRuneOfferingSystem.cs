using System.Linq;
using Content.Server.Bible.Components;
using Content.Server.Body.Systems;
using Content.Server.Cuffs;
using Content.Server.Mind;
using Content.Server.Stunnable;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Server.WhiteDream.BloodCult.Runes.Revive;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;

namespace Content.Server.WhiteDream.BloodCult.Runes.Offering;

public sealed class CultRuneOfferingSystem : EntitySystem
{
    [Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly CuffableSystem _cuffable = default!;
    [Dependency] private readonly CultRuneBaseSystem _cultRune = default!;
    [Dependency] private readonly CultRuneReviveSystem _cultRuneRevive = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CultRuneOfferingComponent, TryInvokeCultRuneEvent>(OnOfferingRuneInvoked);
    }

    private void OnOfferingRuneInvoked(Entity<CultRuneOfferingComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        var possibleTargets = _cultRune.GetTargetsNearRune(ent,
            ent.Comp.OfferingRange,
            entity => HasComp<BloodCultistComponent>(entity));

        if (possibleTargets.Count == 0)
        {
            args.Cancel();
            return;
        }

        var target = possibleTargets.First();

        // if the target is dead we should always sacrifice it.
        if (_mobState.IsDead(target))
        {
            Sacrifice(target);
            return;
        }

        if (!_mind.TryGetMind(target, out _, out _) ||
            _bloodCultRule.IsTarget(target) ||
            HasComp<BibleUserComponent>(target) ||
            HasComp<MindShieldComponent>(target))
        {
            if (!TrySacrifice(target, ent, args.Invokers.Count))
                args.Cancel();

            return;
        }

        if (!TryConvert(target, ent, args.User, args.Invokers.Count))
            args.Cancel();
    }

    private bool TrySacrifice(Entity<HumanoidAppearanceComponent> target,
        Entity<CultRuneOfferingComponent> rune,
        int invokersAmount)
    {
        if (invokersAmount < rune.Comp.AliveSacrificeInvokersAmount)
            return false;

        _cultRuneRevive.AddCharges(rune, rune.Comp.ReviveChargesPerOffering);
        Sacrifice(target);
        return true;
    }

    private void Sacrifice(EntityUid target)
    {
        var transform = Transform(target);
        var shard = Spawn("SoulShard", transform.Coordinates);
        _body.GibBody(target);

        if (!_mind.TryGetMind(target, out var mindId, out _))
            return;

        _mind.TransferTo(mindId, shard);
        _mind.UnVisit(mindId);
    }

    private bool TryConvert(EntityUid target,
        Entity<CultRuneOfferingComponent> rune,
        EntityUid user,
        int invokersAmount)
    {
        if (invokersAmount < rune.Comp.ConvertInvokersAmount)
            return false;

        _cultRuneRevive.AddCharges(rune, rune.Comp.ReviveChargesPerOffering);
        Convert(rune, target, user);
        return true;
    }

    private void Convert(Entity<CultRuneOfferingComponent> rune, EntityUid target, EntityUid user)
    {
        _bloodCultRule.Convert(target);
        _stun.TryStun(target, TimeSpan.FromSeconds(2f), false);
        if (TryComp(target, out CuffableComponent? cuffs) && cuffs.Container.ContainedEntities.Count >= 1)
        {
            var lastAddedCuffs = cuffs.LastAddedCuffs;
            _cuffable.Uncuff(target, user, lastAddedCuffs);
        }

        _statusEffects.TryRemoveStatusEffect(target, "Muted");
        _damageable.TryChangeDamage(target, rune.Comp.ConvertHealing);
    }
}
