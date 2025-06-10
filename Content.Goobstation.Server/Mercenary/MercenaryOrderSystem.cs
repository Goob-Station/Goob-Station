// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later



using Content.Goobstation.Shared.Mercenary;
using Content.Server.Cargo.Systems;
using Robust.Shared.Utility;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Mercenary;

public sealed class MercenaryOrderSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CargoOrderApprovedEvent>(OnCargoOrderApproved);
    }

    private void OnCargoOrderApproved(CargoOrderApprovedEvent ev)
    {
        if (ev.ProductId != "HumanoidMercenary")
            return;

        if (!EntityManager.TryGetEntity(ev.Requester, out var requesterUid))
        {
            Logger.Error($"It fucking failed at {ev.Requester} (Mercenary shit)");
            return;
        }

        var comp = EnsureComp<MercenaryRequesterComponent>(ev.OrderEntity);
        comp.Requester = requesterUid.Value;
        Logger.Info($"It probably fuckin worked huh? (Merc Shit)");
    }

}
