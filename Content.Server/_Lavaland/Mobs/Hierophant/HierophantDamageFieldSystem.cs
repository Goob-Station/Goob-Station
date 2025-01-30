using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using System.Linq;
using Content.Server._Lavaland.Mobs.Hierophant.Components;
using Robust.Shared.Player;

namespace Content.Server._Lavaland.Mobs.Hierophant;

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
            if (TerminatingOrDeleted(uid))
                continue;

            shitters.Add((uid, comp));
        }

        foreach (var shitter in shitters)
        {
            var lookup = _lookup.GetEntitiesInRange(shitter.Item1, .33f)
                .Where(HasComp<ActorComponent>)
                .ToList();

            foreach (var entity in lookup)
            {
                if (!TryComp<DamageableComponent>(entity, out var dmg))
                    continue;

                _dmg.TryChangeDamage(entity, shitter.Item2.Damage, damageable: dmg, targetPart: TargetBodyPart.Torso);
                if (shitter.Item2.Sound != null)
                    _aud.PlayEntity(shitter.Item2.Sound, entity, entity, AudioParams.Default.WithVolume(-3f));
            }

            RemComp(shitter.Item1, shitter.Item2);
        }
    }
}
