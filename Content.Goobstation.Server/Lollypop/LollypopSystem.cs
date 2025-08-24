using Content.Goobstation.Common.Interactions;
using Content.Goobstation.Server.Interaction.Components;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Containers;
using Content.Shared.Clothing;

namespace Content.Goobstation.Server.Lollypop;

public sealed partial class LollypopSystem : EntitySystem
{

    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    [Dependency] private readonly FoodSystem _food = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<LollypopComponent,ClothingGotEquippedEvent>(OnEquipt);
        SubscribeLocalEvent<LollypopComponent,ClothingGotUnequippedEvent>(OnUnequipt);
    }

    public override void Update(float frameTime)
    {
        var query = EntityManager.EntityQueryEnumerator<LollypopComponent, ClothingComponent, FoodComponent>();

        while (query.MoveNext(out var queryUid, out var lollypop, out var clothing, out var food))
        {
            if (clothing.InSlotFlag != SlotFlags.MASK)
                continue;
            if (_time.CurTime < lollypop.NextBite)
                continue;

            Eat((queryUid,lollypop),food);

            lollypop.NextBite = _time.CurTime + lollypop.Interval;
        }
    }
    private void OnEquipt(Entity<LollypopComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ent.Comp.NextBite = _time.CurTime + ent.Comp.Interval;
        ent.Comp.HeldBy = args.Wearer;
    }
    private void OnUnequipt(Entity<LollypopComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.HeldBy = null;
    }
    private void Eat(Entity<LollypopComponent> ent, FoodComponent food)
    {
        if(ent.Comp.HeldBy == null)
            return;

        var oldTransfer = food.TransferAmount;
        food.TransferAmount = ent.Comp.Ammount;

        _food.TryFeed(ent.Owner,ent.Comp.HeldBy.Value,ent.Owner,food);

        food.TransferAmount = oldTransfer;
    }
}
