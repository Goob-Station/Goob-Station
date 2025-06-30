// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Advertise;
using Content.Server.Advertise.Components;
using Content.Shared._DV.VendingMachines;
using Content.Shared.Advertise.Components;
using Content.Shared.Advertise.Systems;

namespace Content.Server._DV.VendingMachines;

public sealed class ShopVendorSystem : SharedShopVendorSystem
{
    [Dependency] private readonly SharedSpeakOnUIClosedSystem _speakOnUIClosed = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShopVendorComponent, TransformComponent>();
        var now = Timing.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            var ent = (uid, comp);
            var dirty = false;
            if (comp.Ejecting is {} ejecting && now > comp.NextEject)
            {
                Spawn(ejecting, xform.Coordinates);
                comp.Ejecting = null;
                dirty = true;
            }

            if (comp.Denying && now > comp.NextDeny)
            {
                comp.Denying = false;
                dirty = true;
            }

            if (dirty)
            {
                Dirty(uid, comp);
                UpdateVisuals(ent);
            }
        }
    }

    protected override void AfterPurchase(Entity<ShopVendorComponent> ent)
    {
        if (TryComp<SpeakOnUIClosedComponent>(ent, out var speak))
            _speakOnUIClosed.TrySetFlag((ent.Owner, speak));
    }
}