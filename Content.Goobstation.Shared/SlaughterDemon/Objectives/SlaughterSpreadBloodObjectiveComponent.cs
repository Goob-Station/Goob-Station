// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.SlaughterDemon.Objectives;

[RegisterComponent]
public sealed partial class SlaughterSpreadBloodObjectiveComponent : Component
{
    [DataField]
    public List<string> Areas = new()
    {
        "Brig",
        "Chapel",
        "Bridge"
    };

    [DataField]
    public string? Title;
}
