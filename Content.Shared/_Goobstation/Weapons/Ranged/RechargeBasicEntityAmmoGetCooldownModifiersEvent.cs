using Robust.Shared.GameObjects;

namespace Content.Shared.Weapons.Ranged.Events;

// todo: get event names closer to the length of the bible
[ByRefEvent]
public record struct RechargeBasicEntityAmmoGetCooldownModifiersEvent(
    float Multiplier
);
