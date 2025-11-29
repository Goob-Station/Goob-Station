using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Adds InfestedComponent to anyone hit by a WhiteTerror spider.
/// </summary>
public sealed class InfestOnHitSystem : EntitySystem
{
    public override void Initialize()
    {
        // Subscribe to melee hit events on any entity
        SubscribeLocalEvent<MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(MeleeHitEvent ev)
    {
        if (!EntityManager.HasComponent<WhiteTerrorComponent>(ev.User))
            return;

        foreach (var target in ev.HitEntities)
        {
            // Ensure the InfestedComponent exists
            EntityManager.EnsureComponent<InfestedComponent>(target);
        }
    }
}
