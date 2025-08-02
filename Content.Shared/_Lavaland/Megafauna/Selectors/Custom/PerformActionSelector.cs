using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

public sealed partial class PerformActionSelector : MegafaunaSelector
{
    [DataField]
    public EntProtoId ActionId;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {

    }
}
