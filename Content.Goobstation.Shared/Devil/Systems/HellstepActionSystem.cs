using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Goobstation.Shared.Devil.EntityEffects;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.Devil.Systems;

public sealed class HellstepActionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellstepActionComponent, DevilHellstepEvent>(TryUse);
    }

    private void TryUse(EntityUid uid, HellstepActionComponent comp, ref DevilHellstepEvent args)
    {
        if (args.Handled)
            return;

        new HellstepEffect().Effect(new EntityEffectBaseArgs(uid, EntityManager));
        args.Handled = true;
    }
}
