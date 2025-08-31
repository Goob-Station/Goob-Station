using Content.Goobstation.Shared.Werewolf.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles bleeding the target on melee hit
/// </summary>
public sealed class BleedOnMeleeHitSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;

    private EntityQuery<BloodstreamComponent> _bloodQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        _bloodQuery = GetEntityQuery<BloodstreamComponent>();

        SubscribeLocalEvent<BleedOnMeleeHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<BleedOnMeleeHitComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        foreach (var entity in args.HitEntities)
        {
            if (!_bloodQuery.TryComp(entity, out var blood))
                return;

            _bloodstream.TryModifyBloodLevel(entity, ent.Comp.BleedAmount, blood);
            _bloodstream.TryModifyBleedAmount(entity, blood.MaxBleedAmount, blood);
        }
    }
}
