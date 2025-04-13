// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Devil;

namespace Content.Goobstation.Server.Devil.Contract;

[RegisterComponent]
public sealed partial class DevilContractComponent : Component
{
    /// <summary>
    /// The entity who signed the paper, AKA, the entity who has the effects applied.
    /// </summary>
    [DataField]
    public EntityUid? Signer;

    /// <summary>
    /// The entity who created the contract, AKA, the entity who gains the soul.
    /// </summary>
    [DataField]
    public EntityUid? ContractOwner;

    /// <summary>
    /// All current clauses.
    /// </summary>
    [DataField]
    public List<DevilClausePrototype> CurrentClauses = new();

    /// <summary>
    /// Has the contract been signed by the signer?
    /// </summary>
    [DataField]
    public bool IsVictimSigned = false;

    /// <summary>
    /// Has the contract been signed by the devil?
    /// </summary>
    [DataField]
    public bool IsDevilSigned = false;

    /// <summary>
    /// Does the contract weigh positively or negatively?
    /// </summary>
    /// <remarks>
    /// The higher it is, the more the cons.
    /// </remarks>
    [DataField]
    public int ContractWeight = 0;
}
