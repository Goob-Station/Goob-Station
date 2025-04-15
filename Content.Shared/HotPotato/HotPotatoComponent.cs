// SPDX-FileCopyrightText: 2023 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.HotPotato;

/// <summary>
/// Similar to <see cref="Interaction.Components.UnremoveableComponent"/>
/// except entities with this component can be removed in specific case: <see cref="CanTransfer"/>
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedHotPotatoSystem))]
public sealed partial class HotPotatoComponent : Component
{
    /// <summary>
    /// If set to true entity can be removed by hitting entities if they have hands
    /// </summary>
    [DataField("canTransfer"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool CanTransfer = true;
}