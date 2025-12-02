using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Terror.Systems;


/// <summary>
/// <code>
///      |/|    < --- Reserved for a specific role :)
///      |/|
///      |/|
///      |/|
///      |/|
///      |/|
///      |/| /¯)
///      |/|/\/
///      |/|\/
///     (¯¯¯)
///     (¯¯¯)
///     (¯¯¯)
///     (¯¯¯)
///     (¯¯¯)
///     /¯¯/\
///    / ,^./\
///   / /   \/\
///  / /     \/\
/// ( (       )/
/// | |       |/|
/// | |       |/|
/// | |       |/|
/// ( (       )/)
///  \ \     / /
///   \ `---' /
///    `-----'
/// </code>
/// </summary>

public sealed class TerrorSpiderSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;


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

        BroadcastSpiderDeath(new Entity<TerrorSpiderComponent>(uid, component));

        var ev = new TerrorSpiderDiedEvent(uid);
        RaiseLocalEvent(uid, ev);
    }

    private void BroadcastSpiderDeath(Entity<TerrorSpiderComponent> deadSpider)
    {
        var query = EntityQueryEnumerator<TerrorSpiderComponent, ActorComponent>();

        while (query.MoveNext(out var spiderPlayerUid, out var comp, out _))
        {
            if (!_net.IsClient)
            {
                _popup.PopupEntity($"A member of the hive has fallen… ({ToPrettyString(deadSpider.Owner)})", spiderPlayerUid, spiderPlayerUid, PopupType.Medium); // LocString broke this, deal with it Russians


                _audio.PlayPredicted(comp.DeathSound, spiderPlayerUid, spiderPlayerUid);
            }
        }
    }
    private void OnWrappedCorpse(EntityUid uid, TerrorSpiderComponent comp, TerrorWrappedCorpseEvent args)
    {
        comp.WrappedAmount++;
        Dirty(uid, comp);

        if (!TryComp(uid, out PassiveDamageComponent? passive))
            return;

        // Initialize baseline once
        if (comp.BaselineRegen == null)
        {
            comp.BaselineRegen = new DamageSpecifier();
            foreach (var (type, value) in passive.Damage.DamageDict)
                comp.BaselineRegen.DamageDict[type] = value;
        }

        var baseline = comp.BaselineRegen;
        var newDamage = new DamageSpecifier();

        // diminishing returns curve
        // k controls curve steepness (3 is good)
        const float k = 3f;
        float capped = comp.MaxRegenCorpses;

        // smooth diminishing returns:
        // approaches capped but never exceeds it
        float effectiveCorpses = capped * (1f - MathF.Exp(-comp.WrappedAmount / k));

        foreach (var (type, value) in baseline.DamageDict)
        {
            // scale based on diminishing-returns result
            var scaled = value * (1 + effectiveCorpses);
            newDamage.DamageDict[type] = scaled;
        }

        passive.Damage = newDamage;
        Dirty(uid, passive);
    }


}
