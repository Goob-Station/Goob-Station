// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.ParticleAccelerator.Components;
using Content.Server.Power.EntitySystems;

namespace Content.Server.ParticleAccelerator.EntitySystems;

public sealed partial class ParticleAcceleratorSystem
{
    private void InitializePowerBoxSystem()
    {
        SubscribeLocalEvent<ParticleAcceleratorPowerBoxComponent, PowerConsumerReceivedChanged>(PowerBoxReceivedChanged);
    }

    private void PowerBoxReceivedChanged(EntityUid uid, ParticleAcceleratorPowerBoxComponent component, ref PowerConsumerReceivedChanged args)
    {
        if (!TryComp<ParticleAcceleratorPartComponent>(uid, out var part))
            return;
        if (!TryComp<ParticleAcceleratorControlBoxComponent>(part.Master, out var controller))
            return;

        var master = part.Master!.Value;
        if (controller.Enabled && args.ReceivedPower >= args.DrawRate * ParticleAcceleratorControlBoxComponent.RequiredPowerRatio)
            PowerOn(master, comp: controller);
        else
            PowerOff(master, comp: controller);

        UpdateUI(master, controller);
    }
}