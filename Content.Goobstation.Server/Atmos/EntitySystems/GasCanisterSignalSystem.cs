// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;

namespace Content.Goobstation.Server.Atmos.EntitySystems;

/// <summary>
/// Handles control signals for automated gas canisters.
/// </summary>
public sealed class GasCanisterSignalSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasCanisterComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnSignalReceived(Entity<GasCanisterComponent> ent, ref SignalReceivedEvent args)
    {
        var valve = args.Port switch
        {
            "Open" => true,
            "Close" => false,
            "Toggle" => !ent.Comp.ReleaseValve,
            _ => false // fuck you c# cant just return
        };

        if (ent.Comp.ReleaseValve == valve)
            return;

        var ev = new GasCanisterChangeReleaseValveMessage(valve);
        ev.UiKey = GasCanisterUiKey.Key;
        if (args.Trigger is {} actor)
            ev.Actor = actor;
        RaiseLocalEvent(ent, ev);
    }
}
