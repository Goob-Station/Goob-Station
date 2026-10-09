// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.MaterialEnergy;

[RegisterComponent]
public sealed partial class MaterialEnergyComponent : Component
{
    [DataField]
    public List<string>? MaterialWhiteList;
}