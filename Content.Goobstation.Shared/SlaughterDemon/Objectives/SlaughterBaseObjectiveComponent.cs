// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.SlaughterDemon.Objectives;

[RegisterComponent]
public sealed partial class SlaughterBaseObjectiveComponent : Component
{
    [DataField]
    public string? Title;

    [DataField]
    public string? Description;
}
