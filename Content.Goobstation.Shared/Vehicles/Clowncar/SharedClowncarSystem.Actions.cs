// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Vehicles.Clowncar;

public abstract partial class SharedClowncarSystem
{
    /// <summary>
    /// Handles activating/deactivating the cannon when requested
    /// </summary>
    private void OnClowncarFireModeAction(EntityUid uid, ClowncarComponent component, ClowncarFireModeActionEvent args)
    {
        if (args.Handled)
            return;

        ToggleCannon(uid, component, args.Performer, true);//component.CannonEntity == null);
        args.Handled = true;
    }
}