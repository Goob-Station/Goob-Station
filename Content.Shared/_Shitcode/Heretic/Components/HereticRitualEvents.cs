// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic.Prototypes;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[Serializable, NetSerializable]
public sealed class HereticRitualMessage(ProtoId<HereticRitualPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<HereticRitualPrototype> ProtoId = protoId;
}

[Serializable, NetSerializable]
public enum HereticRitualRuneUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class HereticShapeshiftMessage(ProtoId<PolymorphPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<PolymorphPrototype> ProtoId = protoId;
}

[Serializable, NetSerializable]
public enum HereticShapeshiftUiKey : byte
{
    Key
}
