using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;

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

        EnsureComp<HellstepComponent>(uid);
        args.Handled = true;
    }
}

