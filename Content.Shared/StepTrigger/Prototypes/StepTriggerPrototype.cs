// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Shared.StepTrigger.Prototypes;

/// <summary>
///     Goobstation Change: Prototype representing a StepTriggerType in YAML.
///     Meant to only have an ID property, as that is the only thing that
///     gets saved in StepTriggerGroup.
/// </summary>
[Prototype]
public sealed partial class StepTriggerTypePrototype : IPrototype
{
    [ViewVariables, IdDataField]
    public string ID { get; private set; } = default!;
}