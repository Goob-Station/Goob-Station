namespace Content.Goobstation.Common.Kitchen;
/// <summary>
/// Raised on the target right before it's being butchered
/// </summary>
/// <param name="Weapon">Weapon used to butcher</param>
[ByRefEvent]
public record struct BeforeBeingButcheredEvent(EntityUid? Weapon);
