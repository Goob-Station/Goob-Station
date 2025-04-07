// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[Serializable, NetSerializable]
public sealed class HereticRitualMessage : BoundUserInterfaceMessage
{
    public ProtoId<HereticRitualPrototype> ProtoId;

    public HereticRitualMessage(ProtoId<HereticRitualPrototype> protoId)
    {
        ProtoId = protoId;
    }
}

[Serializable, NetSerializable]
public enum HereticRitualRuneUiKey : byte
{
    Key
}