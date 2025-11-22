using Content.Shared.CombatMode;
using Content.Shared.MouseRotator;

namespace Content.Goobstation.Shared.CombatMode;

/// <summary>
/// System that initializes <see cref="MouseRotatorComponent"/> based on <see cref="CombatModeComponent"/> when it's added to an entity.
/// </summary>
public sealed class SmoothMouseRotationSystem : EntitySystem
{
    private EntityQuery<CombatModeComponent> _combatQuery; // i love optimizing the code that doesn't even lag for no reason

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MouseRotatorComponent, MapInitEvent>(OnRotatorInit);
        _combatQuery = GetEntityQuery<CombatModeComponent>();
    }

    private void OnRotatorInit(Entity<MouseRotatorComponent> ent, ref MapInitEvent args)
    {
        if (!_combatQuery.TryComp(ent.Owner, out var combat)
            || !combat.SmoothRotation)
            return;

        ent.Comp.AngleTolerance = Angle.FromDegrees(1); // arbitrary
        ent.Comp.Simple4DirMode = false;
    }
}
