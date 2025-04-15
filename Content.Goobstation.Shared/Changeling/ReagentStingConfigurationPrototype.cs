// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling;

[DataDefinition]
[Prototype("reagentStingConfiguration")]
public sealed partial class ReagentStingConfigurationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents = new();
}