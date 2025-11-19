// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Can be inserted into a <see cref="CargoOrderConsoleComponent"/> to increase the station's bank account.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CashComponent : Component
{

}
