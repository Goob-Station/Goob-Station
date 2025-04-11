// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Heretic.Prototypes;

[Serializable, NetSerializable, DataDefinition]
[Prototype("runeCarving")]
public sealed partial class RuneCarvingPrototype : IPrototype, ICloneable
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public SpriteSpecifier Icon;

    [DataField(required: true)]
    public string Desc;

    [DataField(required: true)]
    public EntProtoId ProtoId;

    public object Clone()
    {
        return new RuneCarvingPrototype
        {
            ID = ID,
            Icon = Icon,
            Desc = Desc,
            ProtoId = ProtoId,
        };
    }
}