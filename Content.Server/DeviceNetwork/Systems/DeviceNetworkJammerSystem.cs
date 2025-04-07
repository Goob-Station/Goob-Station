// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Systems;
using Robust.Server.GameObjects;

namespace Content.Server.DeviceNetwork.Systems;

/// <inheritdoc/>
public sealed class DeviceNetworkJammerSystem : SharedDeviceNetworkJammerSystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedDeviceNetworkJammerSystem _jammer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TransformComponent, BeforePacketSentEvent>(BeforePacketSent);
    }

    private void BeforePacketSent(Entity<TransformComponent> xform, ref BeforePacketSentEvent ev)
    {
        if (ev.Cancelled)
            return;

        if (HasComp<MansusGraspAffectedComponent>(ev.SenderTransform.ParentUid)) // Goobstation
        {
            ev.Cancel();
            return;
        }

        var query = EntityQueryEnumerator<DeviceNetworkJammerComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var jammerComp, out var jammerXform))
        {
            if (!_jammer.GetJammableNetworks((uid, jammerComp)).Contains(ev.NetworkId))
                continue;

            if (_transform.InRange(jammerXform.Coordinates, ev.SenderTransform.Coordinates, jammerComp.Range)
                || _transform.InRange(jammerXform.Coordinates, xform.Comp.Coordinates, jammerComp.Range))
            {
                ev.Cancel();
                return;
            }
        }
    }

}