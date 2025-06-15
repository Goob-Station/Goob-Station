// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Shared.Silicon.Bots;

public sealed class FillbotSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _deviceLink = default!;

    [Dependency] private SharedHandsSystem _sharedHandsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FillbotComponent, NewLinkEvent>(OnLinked);
        SubscribeLocalEvent<FillbotComponent, PortDisconnectedEvent>(OnUnlinked);
    }

    // The bot can only be linked to one thing at a time, or it'll freak out.
    private void OnLinked(EntityUid uid, FillbotComponent comp, ref NewLinkEvent args)
    {
        _sharedHandsSystem.TryDrop(uid);
        var newSink = args.Sink;
        var deviceLinkSourceComponent = _entityManager.GetComponent<DeviceLinkSourceComponent>(uid);
        _deviceLink.RemoveAllFromSource(uid, deviceLinkSourceComponent, o => o != newSink);
        comp.LinkedSinkEntity = newSink;
    }

    private void OnUnlinked(EntityUid uid, FillbotComponent comp, ref PortDisconnectedEvent args)
    {
        _sharedHandsSystem.TryDrop(uid);
        var newSink = args.RemovedPortUid;

        if (comp.LinkedSinkEntity == newSink)
            comp.LinkedSinkEntity = null;
    }
}
