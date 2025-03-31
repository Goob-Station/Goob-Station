using Content.Shared.Armor;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Stunnable;

public sealed partial class StaminaDamageResistanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StaminaDamageResistanceComponent, InventoryRelayedEvent<TakeStaminaDamageEvent>>(OnStaminaMeleeHit);
        SubscribeLocalEvent<StaminaDamageResistanceComponent, ArmorExamineEvent>(OnExamine);
    }

    private void OnStaminaMeleeHit(Entity<StaminaDamageResistanceComponent> ent, ref InventoryRelayedEvent<TakeStaminaDamageEvent> args)
    {
        args.Args.Multiplier *= ent.Comp.Coefficient;
    }
    private void OnExamine(Entity<StaminaDamageResistanceComponent> ent, ref ArmorExamineEvent args)
    {
        var percentage = (1 - ent.Comp.Coefficient) * 100;

        if (percentage == 0)
            return;

        args.Msg.PushNewline();
        args.Msg.AddMarkupOrThrow(Loc.GetString("armor-examine-stamina", ("num", percentage)));
    }
}
