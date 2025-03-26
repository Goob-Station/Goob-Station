namespace Content.Shared._Goobstation.Weapons.Ranged;

// todo: get event names closer to the length of the bible
[ByRefEvent]
public record struct RechargeBasicEntityAmmoGetCooldownModifiersEvent(
    float Multiplier
);
