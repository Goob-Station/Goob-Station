using Content.Shared._Goobstation.Weapons.AmmoSelector;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[RegisterComponent]
public sealed partial class ChangelingReagentStingComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ReagentStingConfigurationPrototype> Configuration;

    [DataField]
    public ProtoId<SelectableAmmoPrototype>? DartGunAmmo;
}
