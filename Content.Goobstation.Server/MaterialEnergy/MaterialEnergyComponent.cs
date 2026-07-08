// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.MaterialEnergy;

[RegisterComponent]
public sealed partial class MaterialEnergyComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<string>? MaterialWhiteList;
}