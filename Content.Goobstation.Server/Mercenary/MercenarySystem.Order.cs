// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>Stashed changes
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Cargo;
using Content.Goobstation.Shared.Mercenary;

namespace Content.Goobstation.Server.Mercenary;

public sealed partial class MercenarySystem
{
    public void InitializeOrder()
    {
        SubscribeLocalEvent<CargoOrderApprovedEvent>(OnCargoOrderApproved);
    }

    private void OnCargoOrderApproved(CargoOrderApprovedEvent ev)
    {
        if (ev.ProductId != "HumanoidMercenary")
            return;

        if (!TryGetEntity(ev.Requester, out var requesterUidNullable)
            || requesterUidNullable is not { } requesterUid)
        {
            _sawmill.Error($"Could not resolve : {ev.Requester}");
            return;
        }

        EnsureComp<MercenaryRequesterComponent>(ev.OrderEntity).Requester = requesterUid;
    }

}
