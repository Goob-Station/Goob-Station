// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;

namespace Content.Client.Atmos.EntitySystems;

public sealed class GasTankSystem : SharedGasTankSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasTankComponent, AfterAutoHandleStateEvent>(OnGasTankState);
    }

    private void OnGasTankState(Entity<GasTankComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (UI.TryGetOpenUi(ent.Owner, SharedGasTankUiKey.Key, out var bui))
        {
            bui.Update<GasTankBoundUserInterfaceState>();
        }
    }

    public override void UpdateUserInterface(Entity<GasTankComponent> ent)
    {
        if (UI.TryGetOpenUi(ent.Owner, SharedGasTankUiKey.Key, out var bui))
        {
            bui.Update<GasTankBoundUserInterfaceState>();
        }
    }
}
