// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Objectives.Components;

[RegisterComponent]
public sealed partial class HereticKnowledgeConditionComponent : Component
{
    [DataField] public float Researched = 0f;
}