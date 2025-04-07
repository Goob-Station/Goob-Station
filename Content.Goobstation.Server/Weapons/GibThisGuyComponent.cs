// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Goobstation.Server.Weapons;

[RegisterComponent]
public sealed partial class GibThisGuyComponent : Component
{
    [DataField]
    public List<string> OcNames = new();
    [DataField]
    public List<string> IcNames = new();
    [DataField]
    public bool RequireBoth = false;
}