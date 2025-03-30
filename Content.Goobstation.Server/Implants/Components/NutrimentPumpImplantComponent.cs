namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed class NutrimentPumpImplantComponent : Component
{
    /// <summary>
    /// Did the entity have thirst before being implanted?
    /// </summary>
    [DataField] public bool HadThirst = false;

    /// <summary>
    /// Did the entity have hunger before being implanted?
    /// </summary>
    [DataField] public bool HadHunger = false;
}
