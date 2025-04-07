// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Wires;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Wires;

namespace Content.Server.Access;

public sealed partial class LogWireAction : ComponentWireAction<AccessReaderComponent>
{
    public override Color Color { get; set; } = Color.Blue;
    public override string Name { get; set; } = "wire-name-log";

    [DataField]
    public int PulseTimeout = 30;

    [DataField]
    public LocId PulseLog = "log-wire-pulse-access-log";

    private AccessReaderSystem _access = default!;

    public override StatusLightState? GetLightState(Wire wire, AccessReaderComponent comp)
    {
        return comp.LoggingDisabled ? StatusLightState.Off : StatusLightState.On;
    }

    public override object StatusKey => LogWireActionKey.Status;

    public override void Initialize()
    {
        base.Initialize();

        _access = EntityManager.System<AccessReaderSystem>();
    }

    public override bool Cut(EntityUid user, Wire wire, AccessReaderComponent comp)
    {
        WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
        comp.LoggingDisabled = true;
        EntityManager.Dirty(wire.Owner, comp);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, AccessReaderComponent comp)
    {
        comp.LoggingDisabled = false;
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, AccessReaderComponent comp)
    {
        _access.LogAccess((wire.Owner, comp), Loc.GetString(PulseLog));
        comp.LoggingDisabled = true;
        WiresSystem.StartWireAction(wire.Owner, PulseTimeout, PulseTimeoutKey.Key, new TimedWireEvent(AwaitPulseCancel, wire));
    }

    public override void Update(Wire wire)
    {
        if (!IsPowered(wire.Owner))
            WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
    }

    private void AwaitPulseCancel(Wire wire)
    {
        if (!wire.IsCut && EntityManager.TryGetComponent<AccessReaderComponent>(wire.Owner, out var comp))
            comp.LoggingDisabled = false;
    }

    private enum PulseTimeoutKey : byte
    {
        Key
    }
}
