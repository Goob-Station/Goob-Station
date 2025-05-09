// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.IdentityManagement.Components;
using Content.Shared.Inventory;

namespace Content.Goidastation.Shared.Chemistry;

public sealed partial class GoidaVaporSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdentityBlockerComponent, InventoryRelayedEvent<VaporCheckEyeProtectionEvent>>(OnCheckProtectionEvent);
    }

    private void OnCheckProtectionEvent(Entity<IdentityBlockerComponent> ent, ref InventoryRelayedEvent<VaporCheckEyeProtectionEvent> args)
    {
        if (ent.Comp.Coverage is IdentityBlockerCoverage.MOUTH or IdentityBlockerCoverage.NONE || !ent.Comp.Enabled)
            return;

        args.Args.Protected = true;
    }

}

