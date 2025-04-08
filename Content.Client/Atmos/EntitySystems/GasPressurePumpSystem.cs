// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Atmos.UI;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Atmos.Piping.Binary.Components;

namespace Content.Client.Atmos.EntitySystems;

public sealed class GasPressurePumpSystem : SharedGasPressurePumpSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasPressurePumpComponent, AfterAutoHandleStateEvent>(OnPumpUpdate);
    }

    private void OnPumpUpdate(Entity<GasPressurePumpComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (UserInterfaceSystem.TryGetOpenUi<GasPressurePumpBoundUserInterface>(ent.Owner, GasPressurePumpUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
}