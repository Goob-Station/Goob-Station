// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Marks an entity as a changeling, and holds generic changeling data.
/// For the component holding more complex changeling data, see ChangelingIdentityComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingComponent : Component
{
    [DataField]
    public List<Type> StartingComps = new()
    {
        typeof(ChangelingIdentityComponent)
        // add more starting components here when shit is split from ChangelingIdentity
    };

    [DataField]
    public string MindswapText = "changeling"; // only used for mindswap attempts
}
