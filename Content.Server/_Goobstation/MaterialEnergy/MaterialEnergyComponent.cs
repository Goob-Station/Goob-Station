using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;


namespace Content.Server._Goobstation.MaterialEnergy;

[RegisterComponent]
public sealed partial class MaterialEnergyComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<string>? MaterialWhiteList;
}
