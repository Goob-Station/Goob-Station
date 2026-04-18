using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Handles logic pertaining to a terror spider.
/// </summary>
public sealed class TerrorSpiderSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorSpiderComponent, MobStateChangedEvent>(OnSpiderStateChanged);
        SubscribeLocalEvent<TerrorSpiderComponent, TerrorWrappedCorpseEvent>(OnWrappedCorpse);
    }
    private void OnSpiderStateChanged(EntityUid uid, TerrorSpiderComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (HasComp<TerrorQueenComponent>(uid))
            return;

        BroadcastSpiderDeath((uid, component));

        RaiseLocalEvent(uid, new TerrorSpiderDiedEvent(uid));
    }

    private void BroadcastSpiderDeath(Entity<TerrorSpiderComponent> deadSpider)
    {
        var query = EntityQueryEnumerator<TerrorSpiderComponent, ActorComponent>();

        while (query.MoveNext(out var spiderPlayerUid, out var comp, out _))
        {
            if (spiderPlayerUid == deadSpider.Owner)
                continue;

            _popup.PopupPredicted(Loc.GetString("terror-spider-hive-death", ("spider", deadSpider.Owner)), spiderPlayerUid, spiderPlayerUid, PopupType.Medium);

            _audio.PlayPredicted(comp.DeathSound, spiderPlayerUid, spiderPlayerUid);
        }
    }
    private void OnWrappedCorpse(EntityUid uid, TerrorSpiderComponent comp, ref TerrorWrappedCorpseEvent args)
    {
        comp.WrappedAmount++;
        Dirty(uid, comp);

        if (!TryComp(uid, out PassiveDamageComponent? passive))
            return;

        // Initialize baseline once
        if (comp.BaselineRegen == null)
        {
            comp.BaselineRegen = new DamageSpecifier();

            foreach (var entry in passive.Damage.DamageDict)
            {
                comp.BaselineRegen.DamageDict[entry.Key] = entry.Value;
            }
        }

        var baseline = comp.BaselineRegen;
        var newDamage = new DamageSpecifier();

        // diminishing returns curve
        // k controls curve steepness (3 is good)
        const float k = 3f;
        float capped = comp.MaxRegenCorpses;

        // smooth diminishing returns
        // approaches capped but never exceeds it
        float effectiveCorpses = capped * (1f - MathF.Exp(-comp.WrappedAmount / k));

        foreach (var entry in baseline.DamageDict)
        {
            var scaled = entry.Value * (1 + effectiveCorpses);
            newDamage.DamageDict[entry.Key] = scaled;
        }

        passive.Damage = newDamage;
        if (_netManager.IsServer) // Only should run on server because HiveRule is server only and breaks when this runs in shared code.
            RaiseLocalEvent(uid, new TerrorHiveWrappedEvent());
        Dirty(uid, passive);
    }
}
