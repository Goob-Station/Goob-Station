using Content.Shared.Destructible.Thresholds;

namespace Content.Goobstation.Server.Shizophrenia;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseHallucinationsType
{
    [DataField]
    public MinMax Delay = new();

    public abstract BaseHallucinationsEntry GetEntry();
}
