// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Goobstation.Shared.Devil.Contract;

[RegisterComponent]
public sealed partial class ContractSignerComponent : Component
{
    /// <summary>
    /// The contract entity itself.
    /// </summary>
    [DataField]
    public EntityUid? Contract;

    /// <summary>
    /// The contract component.
    /// </summary>
    [DataField]
    public DevilContractComponent ContractComponent;

    /// <summary>
    /// All current clauses the entity is under the effect of.
    /// </summary>
    [DataField]
    public List<DevilClausePrototype> CurrentClauses = [];

}
