
using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.ModSuits;

[RegisterComponent]
public sealed partial class EnergyAlertComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> PowerAlert = "ModsuitPower";
    public EntityUid? User;
}
