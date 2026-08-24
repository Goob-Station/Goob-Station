// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Mind.Components;

[RegisterComponent]
public sealed partial class IsDeadICComponent : Component
{
    // Goobstation
    [DataField]
    public bool Dead = true;
}

