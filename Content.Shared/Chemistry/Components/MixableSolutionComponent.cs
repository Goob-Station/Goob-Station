// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MixableSolutionComponent : Component
{
    /// <summary>
    /// Solution name which can be mixed with methods such as blessing
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Solution = "default";
}