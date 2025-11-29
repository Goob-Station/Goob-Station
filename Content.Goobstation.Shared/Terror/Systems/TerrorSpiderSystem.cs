using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Terror.Systems;

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
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TerrorSpiderComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            comp.RegenAccumulator += frameTime;

            if (comp.RegenAccumulator < comp.RegenCooldown.TotalSeconds)
                continue;

            comp.RegenAccumulator = 0f;

            if (!TryComp(uid, out DamageableComponent? damageable))
                continue;

            var amount = comp.TerrorRegen * (1 + comp.WrappedAmount);
            _damage.TryChangeDamage(uid, amount, damageable: damageable, targetPart: TargetBodyPart.All);
        }
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
                _popup.PopupEntity($"A member of the hive has fallen… ({ToPrettyString(deadSpider.Owner)})", spiderPlayerUid, spiderPlayerUid,PopupType.Medium);

                _audio.PlayPredicted(comp.DeathSound, spiderPlayerUid, spiderPlayerUid);
            }
        }
    }
}
