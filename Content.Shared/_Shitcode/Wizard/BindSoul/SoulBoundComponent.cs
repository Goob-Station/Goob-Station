// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Robust.Shared.Enums;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.BindSoul;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SoulBoundComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Item;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? MapId;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public string Name = string.Empty;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public int? Age = null;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public Gender? Gender = null;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public Sex? Sex = null;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public int ResurrectionsCount;
}