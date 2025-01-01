using Content.Server.Mind;
using Content.Shared.Species.Components;
using Content.Shared.Body.Events;
using Content.Shared.Zombies;
using Content.Server.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Clothing;
using Content.Shared.Movement.Systems;

namespace Content.Server.Species.Systems;

public sealed partial class YowieSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<YowieComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<YowieComponent, ClothingDidUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<YowieComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
    }

    private void OnEquip(EntityUid uid, YowieComponent comp, ref ClothingDidEquippedEvent args)
    {
        if(args.Clothing.Comp.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
        {
            comp.SuitEquipped = true;
        }
    }
    private void OnUnequip(EntityUid uid, YowieComponent comp, ref ClothingDidUnequippedEvent args)
    {
        if (args.Clothing.Comp.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
        {
            comp.SuitEquipped = false;
        }
    }

    private void OnMove(EntityUid uid, YowieComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.SuitEquipped)
        {
            args.ModifySpeed(component.SpeedMultiplier);
        }
    }
}
