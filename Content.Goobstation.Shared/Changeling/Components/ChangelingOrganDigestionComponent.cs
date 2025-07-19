// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Seven2280 <semvalentin123@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent]
public sealed partial class ChangelingOrganDigestionComponent : Component
{
    /// <summary>
    /// Tag to have the item for chemical recovery
    /// </summary>
    [DataField]
    public string DigestibleTag = "LingEdible";

    /// <summary>
    /// Amount of chemicals received per item eaten
    /// </summary>
    [DataField]
    public float ChemicalsPerItem = 10f;
}
