// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Body.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

/// <summary>
///     Requires that this surgery is (not) done on one of the provided body prototypes
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBodyConditionComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<BodyPrototype>> Accepted = default!;

    [DataField]
    public bool Inverse;
}