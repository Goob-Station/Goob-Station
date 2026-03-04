using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Terror.Systems;

/// <summary>
/// Handles all terror queen spider logic.
/// </summary>
public sealed class TerrorQueenSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorQueenComponent, MobStateChangedEvent>(OnQueenStateChanged);
    }

    private void OnQueenStateChanged(EntityUid uid, TerrorQueenComponent component, MobStateChangedEvent args)
    {
        var rules = EntityQueryEnumerator<TerrorHiveRuleComponent>();

        while (rules.MoveNext(out var ruleUid, out var rule))
        {
            rule.Queen ??= uid;

            Dirty(ruleUid, rule);
        }

        // Only act when the queen *just* died
        if (args.NewMobState != MobState.Dead || args.OldMobState == MobState.Dead)
            return;

        BroadcastQueenDeath(uid, component);
        AffectAllTerrorSpiders(uid, component);
        RaiseLocalEvent(new TerrorSpiderDiedEvent(uid));
    }

    private void BroadcastQueenDeath(EntityUid queenUid, TerrorQueenComponent comp)
    {
        // Create a player filter from all terror spiders with ActorComponent
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<TerrorSpiderComponent, ActorComponent>();

        while (query.MoveNext(out var spiderUid, out _, out var actor))
        {
            if (spiderUid == queenUid)
                continue;

            filter.AddPlayer(actor.PlayerSession);
        }

        if (filter.Count > 0) // plays sound only for spooders that were added to the filter
            _audio.PlayGlobal(comp.DeathSound, filter, false);
    }

    private void AffectAllTerrorSpiders(EntityUid queenUid, TerrorQueenComponent comp)
    {
        var query = EntityQueryEnumerator<TerrorSpiderComponent>();

        while (query.MoveNext(out var spiderUid, out _))
        {
            if (spiderUid == queenUid)
                continue;

            if (_random.Prob(comp.DeathGibChance))
            {
                // 50% chance: gib the spider
                _popup.PopupEntity(
                    Loc.GetString("queen-death-gib"),
                    spiderUid,
                    spiderUid,
                    PopupType.LargeCaution);
                _body.GibBody(spiderUid);
            }
            else
            {
                // Otherwise: give berserker rage
                _popup.PopupEntity(
                    Loc.GetString("queen-death-rage"),
                    spiderUid,
                    spiderUid,
                    PopupType.Medium);
                EnsureComp<BerserkerRageComponent>(spiderUid);
            }
        }
    }
}
