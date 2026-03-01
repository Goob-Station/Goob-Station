// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Marks an entity as a changeling, and holds generic changeling data.
/// For the component holding more complex changeling data, see ChangelingIdentityComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingComponent : Component
{
    /// <summary>
    /// The starting components to be assigned to a changeling.
    /// </summary>
    [DataField]
    public ProtoId<ChangelingStartingEvolutionPrototype> EvolutionsProto = "DefaultChangeling";

    /// <summary>
    /// Have the components been assigned?
    /// </summary>
    [DataField]
    public bool EvolutionsAssigned;

    [DataField]
    public string MindswapText = "changeling"; // only used for mindswap attempts
}
