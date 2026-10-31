// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Nuke;

[RegisterComponent]
public sealed partial class NukeWreckCodeComponent : Component
{
    [DataField]
    public string? Code;
}
