using Content.Shared.Inventory;

namespace Content.Shared.Weapons.Melee.Events;

[ByRefEvent]
public record struct GetBonusDisarmChanceEvent(float Modifier) : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots =>  ~SlotFlags.POCKET;
}
//john station A.K john cord & motherfucker fadeokno helped me with this shit, i dont give a fuck how in fuck this fucking fuck works but fcukign fuck thank them god blessed DONT TOUCH its magic