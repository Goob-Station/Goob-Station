using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// AoE stun.
/// </summary>
public sealed class TerrorTremorSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private readonly HashSet<Entity<StatusEffectsComponent>> _targets = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorTremorComponent, TerrorTremorEvent>(OnTremor);
    }

    private void OnTremor(Entity<TerrorTremorComponent> ent, ref TerrorTremorEvent args)
    {
        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.Range, _targets);

        foreach (var target in _targets)
        {
            if (target.Owner == ent.Owner)
            {
                continue;
            }

            if (HasComp<TerrorSpiderComponent>(target.Owner))
            {
                continue;
            }

            _stun.TryAddStunDuration(target.Owner, ent.Comp.Stun);
            _stun.TryKnockdown(target.Owner, ent.Comp.Knockdown, true);
        }

        _audio.PlayPredicted(ent.Comp.Sound, ent.Owner, ent.Owner);

        args.Handled = true;
    }
}
