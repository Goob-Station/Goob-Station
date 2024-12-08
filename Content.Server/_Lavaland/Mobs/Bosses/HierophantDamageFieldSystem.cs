using Content.Server._Lavaland.Mobs.Bosses.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using System.Linq;

namespace Content.Server._Lavaland.Mobs.Bosses;

/// <summary>
///     We have to use it's own system even for the damage field because WIZDEN SYSTEMS FUCKING SUUUUUUUUUUUCKKKKKKKKKKKKKKK
/// </summary>
public sealed partial class HierophantDamageFieldSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HierophantDamageFieldComponent>();
        var shitters = new List<(EntityUid, HierophantDamageFieldComponent)>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            shitters.Add((uid, comp));
        }

        foreach (var shitter in shitters)
        {
            var lookup = _lookup.GetEntitiesInRange(shitter.Item1, .25f)
                .Where(p => !HasComp<HierophantBossComponent>(p)).ToList();
            foreach (var entity in lookup)
            {
                if (TryComp<DamageableComponent>(entity, out var dmg))
                    _dmg.TryChangeDamage(entity, shitter.Item2.Damage, damageable: dmg, targetPart: TargetBodyPart.Torso);

                if (shitter.Item2.Sound != null)
                    _aud.PlayPvs(shitter.Item2.Sound, shitter.Item1, AudioParams.Default.WithMaxDistance(5f).WithVolume(-10f));
            }
            RemComp(shitter.Item1, shitter.Item2);
        }
    }
}
