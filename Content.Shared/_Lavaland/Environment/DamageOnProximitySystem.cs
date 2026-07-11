using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Environment;

/// <summary>
/// Because DamageOnTrigger + TriggerOnProximity does not fit quite right. Deals damage in a set radius at a set interval.
/// </summary>
public sealed class DamageOnProximitySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    private readonly HashSet<(EntityUid Victim, string Group)> _recentlyDamaged = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<DamageOnProximityComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (curTime < comp.NextDamageTime)
                continue;

            comp.NextDamageTime = curTime + TimeSpan.FromSeconds(comp.Interval);

            foreach (var ent in _lookup.GetEntitiesInRange(xform.Coordinates, comp.Range))
            {
                if (!HasComp<DamageableComponent>(ent))
                    continue;

                if (_whitelist.IsWhitelistPass(comp.Blacklist, ent))
                    continue;

                var key = (ent, comp.Group);

                if (!_recentlyDamaged.Add(key))
                    continue;

                _damageable.TryChangeDamage(ent, comp.Damage, true, interruptsDoAfters: false);
                if (comp.DamageSound is not null)
                {
                    _audio.PlayPvs(comp.DamageSound, ent, null);
                }
                Timer.Spawn(TimeSpan.FromSeconds(comp.Interval), () => _recentlyDamaged.Remove(key));
            }
        }
    }
}
