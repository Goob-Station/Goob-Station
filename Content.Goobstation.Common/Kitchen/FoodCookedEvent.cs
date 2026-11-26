namespace Content.Goobstation.Common.Kitchen;

[ByRefEvent]
public readonly record struct FoodCookedEvent(EntityUid Microwave, int Count);
