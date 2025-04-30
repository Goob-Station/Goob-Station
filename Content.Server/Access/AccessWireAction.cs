// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 deltanedas <deltanedas@laptop>
// SPDX-FileCopyrightText: 2023 deltanedas <user@zenith>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Wires;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Wires;

namespace Content.Server.Access;

public sealed partial class AccessWireAction : ComponentWireAction<AccessReaderComponent>
{
    public override Color Color { get; set; } = Color.Green;
    public override string Name { get; set; } = "wire-name-access";

    [DataField("pulseTimeout")]
    private int _pulseTimeout = 30;

    public override StatusLightState? GetLightState(Wire wire, AccessReaderComponent comp)
    {
        return comp.Enabled ? StatusLightState.On : StatusLightState.Off;
    }

    public override object StatusKey => AccessWireActionKey.Status;

    public override bool Cut(EntityUid user, Wire wire, AccessReaderComponent comp)
    {
        WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
        comp.Enabled = false;
        EntityManager.Dirty(wire.Owner, comp);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, AccessReaderComponent comp)
    {
        comp.Enabled = true;
        EntityManager.Dirty(wire.Owner, comp);

        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, AccessReaderComponent comp)
    {
        comp.Enabled = false;
        EntityManager.Dirty(wire.Owner, comp);
        WiresSystem.StartWireAction(wire.Owner, _pulseTimeout, PulseTimeoutKey.Key, new TimedWireEvent(AwaitPulseCancel, wire));
    }

    public override void Update(Wire wire)
    {
        if (!IsPowered(wire.Owner))
        {
            WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
        }
    }

    private void AwaitPulseCancel(Wire wire)
    {
        if (!wire.IsCut)
        {
            if (EntityManager.TryGetComponent<AccessReaderComponent>(wire.Owner, out var access))
            {
                access.Enabled = true;
                EntityManager.Dirty(wire.Owner, access);
            }
        }
    }

    private enum PulseTimeoutKey : byte
    {
        Key
    }
}