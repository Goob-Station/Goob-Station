using Content.Goobstation.Shared.EntityEffects;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Components;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class AddFireStacksEntityEffectSystem : EntityEffectSystem<FlammableComponent, AddFireStacks>
{
    [Dependency] private FlammableSystem _flammable = default!;

    protected override void Effect(Entity<FlammableComponent> ent, ref EntityEffectEvent<AddFireStacks> args)
    {
        _flammable.AdjustFireStacks(ent.Owner, args.Effect.Stacks, ent.Comp);
    }
}
