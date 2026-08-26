using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Terror.Systems;

/// <summary>
/// Ticks damage and periodic stuns while infested, then bursts into a spiderling with a long
/// stun once the burst timer's up, clearing the status.
/// </summary>
public sealed class InfestedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InfestedComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, InfestedComponent comp, MapInitEvent args)
    {
        var now = _timing.CurTime;
        comp.NextDamageTick = now + comp.DamageInterval;
        comp.NextStunTick = now + comp.StunInterval;
        comp.BurstAt = now + comp.BurstDelay;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<InfestedComponent, StatusEffectComponent>();

        while (query.MoveNext(out var uid, out var comp, out var status))
        {
            if (status.AppliedTo is not { } target) continue;

            var now = _timing.CurTime;

            if (now >= comp.BurstAt)
            {
                Burst(comp, target);
                continue;
            }

            if (now >= comp.NextDamageTick)
            {
                comp.NextDamageTick = now + comp.DamageInterval;
                _damageable.TryChangeDamage(target, comp.Damage, origin: target);
                _popup.PopupEntity(Loc.GetString("infested-shakes"), target, target, PopupType.MediumCaution);
            }

            if (now >= comp.NextStunTick)
            {
                comp.NextStunTick = now + comp.StunInterval;
                _stun.TryAddStunDuration(target, comp.StunDuration);
            }
        }
    }

    private void Burst(InfestedComponent comp, EntityUid target)
    {
        _stun.TryAddStunDuration(target, comp.BurstStun);
        _stun.TryKnockdown(target, comp.BurstStun, true);

        if (comp.SpiderlingPrototypes.Count > 0)
        {
            var protoId = comp.SpiderlingPrototypes[_random.Next(comp.SpiderlingPrototypes.Count)];
            Spawn(protoId, Transform(target).Coordinates);
        }

        _audio.PlayPredicted(comp.SpawnSound, target, target);

        _status.TryRemoveStatusEffect(target, "Infested");
    }
}
