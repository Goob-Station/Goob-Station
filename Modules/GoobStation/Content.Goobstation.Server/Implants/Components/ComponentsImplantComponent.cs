// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Scruq445 <storchdamien@gmail.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
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
