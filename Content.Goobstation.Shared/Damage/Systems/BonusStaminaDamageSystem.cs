using Content.Goobstation.Common.Damage.Events;
using Content.Goobstation.Shared.Damage.Components;

namespace Content.Goobstation.Shared.Damage.System;

public sealed class BonusStaminaDamageSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BonusStaminaDamageComponent, ModifyOutgoingStaminaDamageEvent>(OnModifyOutgoingStamina);
    }

    private void OnModifyOutgoingStamina(Entity<BonusStaminaDamageComponent> ent, ref ModifyOutgoingStaminaDamageEvent args)
        => args.Value *= ent.Comp.Multiplier;
}
