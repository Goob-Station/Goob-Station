// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SpeltIncorrectyl <66873282+SpeltIncorrectyl@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Paper;

/// <summary>
/// An entity with this component cannot write on paper.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlockWritingComponent : Component
{
    /// <summary>
    /// What message is displayed when the entity fails to write?
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public LocId FailWriteMessage = "paper-component-illiterate";
}