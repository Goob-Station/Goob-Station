/*
using Content.Shared._Goobstation.Clothing;
using Content.Shared._Goobstation.Flashbang;
using Content.Shared._Goobstation.Wizard.Chuuni;
using Content.Shared._White.Overlays;
using Content.Shared.Damage.Events;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.Inventory;


public sealed class GoobInventorySystem : InventorySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InventoryComponent, DelayedKnockdownAttemptEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, CheckMagicItemEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetFlashbangedEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, FlashDurationMultiplierEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetStandingUpTimeMultiplierEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetSpellInvocationEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, GetMessagePostfixEvent>(RelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, ModifyStunTimeEvent>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<NightVisionComponent>>(RefRelayInventoryEvent);
        SubscribeLocalEvent<InventoryComponent, TakeStaminaDamageEvent>(RelayInventoryEvent); // goobstation - stun resistance
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<ThermalVisionComponent>>(RefRelayInventoryEvent);
    }
}
FUCK
*/
