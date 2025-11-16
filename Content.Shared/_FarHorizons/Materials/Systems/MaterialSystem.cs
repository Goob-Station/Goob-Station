namespace Content.Shared._FarHorizons.Materials.Systems;

public sealed class MaterialSystem
{
    public static double CalculateHeatTransferCoefficient(MaterialProperties? materialA, MaterialProperties? materialB)
    {
        var hTC1 = 5.0;
        var hTC2 = 5.0;

        if (materialA != null)
            if (materialA.ThermalConductivity > 0 && materialA.ElectricalConductivity > 0)
                hTC1 = (Math.Max(materialA.ThermalConductivity, 0) + Math.Max(materialA.ElectricalConductivity, 0)) / 2;
            else if (materialA.ThermalConductivity > 0)
                hTC1 = Math.Max(materialA.ThermalConductivity, 0);
            else if (materialA.ElectricalConductivity > 0)
                hTC1 = Math.Max(materialA.ElectricalConductivity, 0);
        if (materialB != null)
            if (materialB.ThermalConductivity > 0 && materialB.ElectricalConductivity > 0)
                hTC2 = (Math.Max(materialB.ThermalConductivity, 0) + Math.Max(materialB.ElectricalConductivity, 0)) / 2;
            else if (materialB.ThermalConductivity > 0)
                hTC2 = Math.Max(materialB.ThermalConductivity, 0);
            else if (materialB.ElectricalConductivity > 0)
                hTC2 = Math.Max(materialB.ElectricalConductivity, 0);

        return ((Math.Pow(10, hTC1 / 5) - 1) + (Math.Pow(10, hTC2 / 5) - 1)) / 2;
    }
}