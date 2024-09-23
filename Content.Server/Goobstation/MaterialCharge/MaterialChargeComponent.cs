using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;


namespace Content.Server.Goobstation.MaterialCharge;

[RegisterComponent]
public sealed partial class MaterialChargeComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<string>? MaterialWhiteList;
}