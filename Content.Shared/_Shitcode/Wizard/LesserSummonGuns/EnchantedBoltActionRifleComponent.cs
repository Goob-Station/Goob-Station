// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.LesserSummonGuns;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EnchantedBoltActionRifleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Caster;

    [DataField, AutoNetworkedField]
    public int Shots = 30;

    [DataField]
    public EntProtoId Proto = "WeaponBoltActionEnchanted";

    [DataField]
    public Vector2 ThrowingSpeed = new(2f, 4f);
}