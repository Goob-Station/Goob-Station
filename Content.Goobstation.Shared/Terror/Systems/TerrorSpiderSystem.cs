using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Goobstation.Shared.Terror.Prototypes;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Main system of Terror spiders.
/// </summary>
public sealed class TerrorSpiderSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SpiderEggLayerSystem _eggLayer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorSpiderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TerrorSpiderComponent, MobStateChangedEvent>(OnStateChanged);
        SubscribeLocalEvent<TerrorSpiderComponent, TerrorWrappedCorpseEvent>(OnWrappedCorpse);
    }

    private void OnMapInit(EntityUid uid, TerrorSpiderComponent comp, MapInitEvent args)
    {
        if (!_proto.TryIndex(comp.SpiderType, out var proto))
            return;

        var rules = EntityQueryEnumerator<TerrorHiveRuleComponent>();

        while (rules.MoveNext(out var ruleUid, out var rule))
        {
            if (proto.IsQueen)
                rule.Queen ??= uid;
            else
                comp.Queen ??= rule.Queen;

            Dirty(ruleUid, rule);
        }
    }

    private void OnStateChanged(EntityUid uid, TerrorSpiderComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.OldMobState == MobState.Dead)
            return;

        if (!_proto.TryIndex(comp.SpiderType, out var proto))
            return;

        BroadcastDeath(uid, proto);

        if (proto.IsQueen)
            AffectHiveOnQueenDeath(uid);

        RaiseLocalEvent(uid, new TerrorSpiderDiedEvent(uid));
    }

    private void BroadcastDeath(EntityUid uid, TerrorSpiderPrototype proto)
    {
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<TerrorSpiderComponent, ActorComponent>();

        while (query.MoveNext(out var spiderUid, out _, out var actor))
        {
            if (spiderUid == uid)
                continue;

            filter.AddPlayer(actor.PlayerSession);
        }

        if (filter.Count == 0)
            return;

        var locKey = proto.IsQueen ? "terror-hive-queen-death" : "terror-spider-hive-death";

        foreach (var session in filter.Recipients)
        {
            if (session.AttachedEntity is not { } listener)
                continue;

            _popup.PopupEntity(Loc.GetString(locKey, ("spider", uid)), listener, listener, PopupType.Medium);
        }

        _audio.PlayGlobal(proto.DeathSound, filter, false);
    }

    private void AffectHiveOnQueenDeath(EntityUid queenUid)
    {
        if (_netManager.IsClient)
            return;

        var query = EntityQueryEnumerator<TerrorSpiderComponent>();

        while (query.MoveNext(out var spiderUid, out var comp))
        {
            if (spiderUid == queenUid)
                continue;

            if (_random.Prob(comp.QueenDeathGibChance))
            {
                _popup.PopupEntity(Loc.GetString("queen-death-gib"), spiderUid, spiderUid, PopupType.LargeCaution);
                _body.GibBody(spiderUid);
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("queen-death-rage"), spiderUid, spiderUid, PopupType.Medium);
                EnsureComp<BerserkerRageComponent>(spiderUid);
            }
        }
    }

    private void OnWrappedCorpse(EntityUid uid, TerrorSpiderComponent comp, ref TerrorWrappedCorpseEvent args)
    {
        comp.WrappedAmount++;
        Dirty(uid, comp);

        if (TryComp(uid, out PassiveDamageComponent? passive))
            ScaleRegen(comp, passive);

        if (_netManager.IsClient)
            return;

        if (_proto.TryIndex(comp.SpiderType, out var proto) && proto.IsEggLayer)
            _eggLayer.AddEgg(uid);

        RaiseLocalEvent(uid, new TerrorHiveWrappedEvent());
    }

    private void ScaleRegen(TerrorSpiderComponent comp, PassiveDamageComponent passive)
    {
        comp.BaselineRegen ??= new DamageSpecifier(passive.Damage);

        var baseline = comp.BaselineRegen;
        var newDamage = new DamageSpecifier();

        const float k = 3f;
        var effectiveCorpses = comp.MaxRegenCorpses * (1f - MathF.Exp(-comp.WrappedAmount / k));

        foreach (var (type, value) in baseline.DamageDict)
        {
            newDamage.DamageDict[type] = value * (1 + effectiveCorpses);
        }

        passive.Damage = newDamage;
        Dirty(passive.Owner, passive);
    }
}
