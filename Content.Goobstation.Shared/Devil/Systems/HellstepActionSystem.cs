using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Goobstation.Shared.Devil.EntityEffects;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.Devil.Systems;

public sealed class HellstepActionSystem : EntitySystem
{
    [Dependency] private readonly SharedEntityEffectsSystem _entityEffects = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HellstepActionComponent, DevilHellstepEvent>(TryUse);
    }

    private void TryUse(EntityUid uid, HellstepActionComponent comp, ref DevilHellstepEvent args)
    {
        if (args.Handled)
            return;

        _entityEffects.ApplyEffect(uid, new HellstepEffect());
        args.Handled = true;
    }
}
