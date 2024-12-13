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

    private void OnOfferingRuneInvoked(Entity<CultRuneOfferingComponent> rune, ref TryInvokeCultRuneEvent args)
    {
        var possibleTargets = _cultRune.GetTargetsNearRune(
            rune,
            rune.Comp.OfferingRange,
            entity => HasComp<BloodCultistComponent>(entity));

        if (possibleTargets.Count == 0)
        {
            args.Cancel();
            return;
        }

        var target = possibleTargets.First();
        if (!TryOffer(rune, target, args.User, args.Invokers.Count))
            args.Cancel();
    }

    private bool TryOffer(Entity<CultRuneOfferingComponent> rune, EntityUid target, EntityUid user, int invokersTotal)
    {
        // if the target is dead we should always sacrifice it.
        if (_mobState.IsDead(target))
        {
            Sacrifice(rune, target);
            return true;
        }

        if (!_mind.TryGetMind(target, out _, out _) || _bloodCultRule.IsTarget(target) ||
            HasComp<BibleUserComponent>(target) || HasComp<MindShieldComponent>(target))
            return TrySacrifice(rune, target, invokersTotal);

        return TryConvert(rune, target, user, invokersTotal);
    }

    private bool TrySacrifice(Entity<CultRuneOfferingComponent> rune, EntityUid target, int invokersAmount)
    {
        if (invokersAmount < rune.Comp.AliveSacrificeInvokersAmount)
            return false;

        Sacrifice(rune, target);
        return true;
    }

    private bool TryConvert(Entity<CultRuneOfferingComponent> rune, EntityUid target, EntityUid user, int invokersTotal)
    {
        if (invokersTotal < rune.Comp.ConvertInvokersAmount)
            return false;

        _cultRuneRevive.AddCharges(rune, rune.Comp.ReviveChargesPerOffering);
        Convert(rune, target, user);
        return true;
    }

    private void Sacrifice(Entity<CultRuneOfferingComponent> rune, EntityUid target)
    {
        _cultRuneRevive.AddCharges(rune, rune.Comp.ReviveChargesPerOffering);
        var transform = Transform(target);

        if (!_mind.TryGetMind(target, out var mindId, out _))
            Spawn(rune.Comp.SoulShardGhostProto, transform.Coordinates);
        else
        {
            var shard = Spawn(rune.Comp.SoulShardProto, transform.Coordinates);
            _mind.TransferTo(mindId, shard);
            _mind.UnVisit(mindId);
        }

        _body.GibBody(target);
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
