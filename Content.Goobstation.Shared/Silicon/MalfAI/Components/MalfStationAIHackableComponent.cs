// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Silicon.MalfAI.Components;

[RegisterComponent]
public sealed partial class MalfStationAIHackableComponent : Component
{
    [DataField]
    public bool Hacked = false;

    [DataField]
    public TimeSpan SecondsToHack = TimeSpan.FromSeconds(10);
}