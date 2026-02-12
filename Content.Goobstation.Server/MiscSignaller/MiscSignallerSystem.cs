// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SomethingUnbearable <mewatcher102@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DeviceLinking.Systems;
using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.MiscSignaller;

public sealed class MiscSignallerSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _link = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MiscSignallerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MiscSignallerComponent, TriggerEvent>(OnTrigger);
    }
    private void OnInit(EntityUid uid, MiscSignallerComponent component, ComponentInit args)
        => _link.EnsureSourcePorts(uid, component.Port);

    private void OnTrigger(EntityUid uid, MiscSignallerComponent component, TriggerEvent args)
    {
        if (component.NextActivationWindow > _timing.CurTime)
        {
            args.Handled = true;
            return;
        }
        _link.InvokePort(uid, component.Port);
        args.Handled = true;
        component.NextActivationWindow = _timing.CurTime + component.ActivationInterval;
    }
}