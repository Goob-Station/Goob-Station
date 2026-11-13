using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared.Administration.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class Rejuvenate : EntityEffectBase<Rejuvenate>;

public sealed partial class RejuvenateEntityEffectSystem : EntityEffectSystem<MobStateComponent, Rejuvenate>
{
    [Dependency] private RejuvenateSystem _rejuvenate = default!;

    protected override void Effect(Entity<MobStateComponent> ent, ref EntityEffectEvent<Rejuvenate> args)
    {
        _rejuvenate.PerformRejuvenate(ent);
    }
}
