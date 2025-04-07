// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class GrantImplantComponent : Component
{
    [DataField] public HashSet<String> Implants { get; private set; } = new();
}