// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceLinking.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Server.DeviceLinking.Systems;

public sealed partial class GunSignalControlSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GunSignalControlComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<GunSignalControlComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnInit(Entity<GunSignalControlComponent> gunControl, ref MapInitEvent args)
    {
        _signalSystem.EnsureSinkPorts(gunControl, gunControl.Comp.TriggerPort, gunControl.Comp.TogglePort, gunControl.Comp.OnPort, gunControl.Comp.OffPort);
    }

    private void OnSignalReceived(Entity<GunSignalControlComponent> gunControl, ref SignalReceivedEvent args)
    {
        if (!TryComp<GunComponent>(gunControl, out var gun))
            return;

        if (args.Port == gunControl.Comp.TriggerPort)
            _gun.AttemptShoot(gunControl, gun);

        if (!TryComp<AutoShootGunComponent>(gunControl, out var autoShoot))
            return;

        if (args.Port == gunControl.Comp.TogglePort)
           _gun.SetEnabled(gunControl, autoShoot, !autoShoot.Enabled);

        if (args.Port == gunControl.Comp.OnPort)
            _gun.SetEnabled(gunControl, autoShoot, true);

        if (args.Port == gunControl.Comp.OffPort)
            _gun.SetEnabled(gunControl, autoShoot, false);
    }
}