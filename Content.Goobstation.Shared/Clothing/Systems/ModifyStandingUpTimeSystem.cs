using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared._Goobstation.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Clothing.Systems;

public sealed class MultiplyStandingUpTimeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Goobstation.Shared.Clothing.Components.ModifyStandingUpTimeComponent, GetStandingUpTimeMultiplierEvent>(OnGetMultiplier);
        SubscribeLocalEvent<Goobstation.Shared.Clothing.Components.ModifyStandingUpTimeComponent, InventoryRelayedEvent<GetStandingUpTimeMultiplierEvent>>(
            OnInventoryGetMultiplier);
        SubscribeLocalEvent<Goobstation.Shared.Clothing.Components.ModifyStandingUpTimeComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<Goobstation.Shared.Clothing.Components.ModifyStandingUpTimeComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<ClothingComponent>(ent))
            return;

        var msg = Loc.GetString("clothing-modify-stand-up-time-examine",
            ("mod", MathF.Round((1f - ent.Comp.Multiplier) * 100)));
        args.PushMarkup(msg);
    }

    private void OnInventoryGetMultiplier(Entity<Goobstation.Shared.Clothing.Components.ModifyStandingUpTimeComponent> ent, ref InventoryRelayedEvent<GetStandingUpTimeMultiplierEvent> args)
    {
        args.Args.Multiplier *= ent.Comp.Multiplier;
    }

    private void OnGetMultiplier(Entity<Goobstation.Shared.Clothing.Components.ModifyStandingUpTimeComponent> ent, ref GetStandingUpTimeMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }
}
