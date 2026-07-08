// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Database;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Administration.UI.CustomControls;

public sealed class AdminLogImpactButton : Button
{
    public AdminLogImpactButton(LogImpact impact)
    {
        Impact = impact;
        ToggleMode = true;
        Pressed = true;
    }

    public LogImpact Impact { get; }
}