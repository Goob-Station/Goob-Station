// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Fax;
using Content.Shared.Fax.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Fax;

/// <summary>
/// Handles signals for automated fax machines.
/// </summary>
public sealed class FaxSignalSystem : EntitySystem
{
    public static readonly ProtoId<SinkPortPrototype> CopyPort = "FaxCopy";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaxMachineComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnSignalReceived(Entity<FaxMachineComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == CopyPort)
            RaiseLocalEvent(ent, new FaxCopyMessage());
    }
}
