namespace Content.Goobstation.Shared.PlasmaCutterFuel;

[RegisterComponent]
public sealed partial class PlasmaCutterFuelComponent : Component
{
    [DataField("energyPerSheet")]
    public int EnergyPerSheet;
}