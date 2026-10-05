namespace Content.Goobstation.Common.AlmanacBlade;

[ByRefEvent]
public readonly record struct PlantHarvestedEvent;

[ByRefEvent]
public readonly record struct GetHiddenCargoProductsEvent(HashSet<string> Hidden);
