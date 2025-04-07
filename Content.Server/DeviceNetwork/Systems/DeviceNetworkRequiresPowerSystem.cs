// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.DeviceNetwork.Components;
using Content.Server.Power.EntitySystems;

namespace Content.Server.DeviceNetwork.Systems;

public sealed class DeviceNetworkRequiresPowerSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<DeviceNetworkRequiresPowerComponent, BeforePacketSentEvent>(OnBeforePacketSent);
    }

    private void OnBeforePacketSent(EntityUid uid, DeviceNetworkRequiresPowerComponent component,
        BeforePacketSentEvent args)
    {
        if (!this.IsPowered(uid, EntityManager))
        {
            args.Cancel();
        }
    }
}