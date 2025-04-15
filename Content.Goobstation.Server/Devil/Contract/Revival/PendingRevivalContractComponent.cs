// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Devil.Contract.Revival;

[RegisterComponent]
public sealed partial class PendingRevivalContractComponent : Component
{
    /// <summary>
    /// The entity being revived.
    /// </summary>
    [DataField]
    public EntityUid? Contractee;

    /// <summary>
    /// The entity offering revival
    /// </summary>
    [DataField]
    public EntityUid? Offerer;

    /// <summary>
    /// How long until the deal expires?
    /// </summary>
    [DataField]
    public TimeSpan ExpiryTime = TimeSpan.FromSeconds(45);
}
