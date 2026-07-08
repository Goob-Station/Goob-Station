// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Weapons.AmmoSelector;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent]
public sealed partial class ChangelingReagentStingComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ReagentStingConfigurationPrototype> Configuration;

    [DataField]
    public ProtoId<SelectableAmmoPrototype>? DartGunAmmo;
}