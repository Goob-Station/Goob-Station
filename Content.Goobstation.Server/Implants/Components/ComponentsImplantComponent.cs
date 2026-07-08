// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Implants.Components;

/// <summary>
/// Adds or removes components to the implanted mob.
/// </summary>
[RegisterComponent]
public sealed partial class ComponentsImplantComponent : Component
{
    [DataField]
    public ComponentRegistry? Added;

    [DataField]
    public ComponentRegistry? Removed;
}
