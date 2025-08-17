// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 unknown <Administrator@DESKTOP-PMRIVVA.kommune.indresogn.no>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bingle;

[RegisterComponent, NetworkedComponent]
public sealed partial class BingleComponent : Component
{
    [DataField]
    public bool Upgraded = false;
    [DataField]
    public bool Prime = false;
    [DataField]
    public EntityUid? MyPit;
    [DataField]
    public EntProtoId? UpgradedID;
}

[Serializable, NetSerializable]
public enum BingleVisual : byte
{
    Upgraded,
    Combat
}
