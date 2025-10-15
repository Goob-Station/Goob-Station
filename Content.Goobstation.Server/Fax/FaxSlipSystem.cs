// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Fax;
using Content.Goobstation.Shared.Lube;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Fax;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.Fax.Components;
using Content.Shared.Lube;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Fax;

public sealed class FaxSlipSystem : EntitySystem
{
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly IRobustRandom _gambling = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaxSlipComponent, GettingFaxedSentEvent>(OnGettingFaxedSent);
        SubscribeLocalEvent<FaxSlipComponent, CanLubedInsertEvent>(OnCanLubedInsert);
    }

    private void OnGettingFaxedSent(Entity<FaxSlipComponent> ent, ref GettingFaxedSentEvent args)
    {
        var chance = HasComp<LubedComponent>(ent) && ent.Comp.LubedChance != null ? ent.Comp.LubedChance.Value : ent.Comp.SlipChance;
        var shouldSlip = _gambling.Prob(chance);

        // FaxSystem wasn't really intended to do this so this copypastes logic from Send()
        // justifiable since other listeners to GettingFaxedSentEvent() might want to do different logic
        if (shouldSlip)
        {
            // stop normal faxing behaviors
            args.Handled = true;

            // FaxSystem should probably be changed to handle this by itself
            if (args.Fax.Comp.SendTimeoutRemaining > 0)
                return;

            var sendEntity = args.Fax.Comp.PaperSlot.Item;
            if (sendEntity == null)
                return;

            if (args.Fax.Comp.DestinationFaxAddress == null)
                return;

            if (!args.Fax.Comp.KnownFaxes.TryGetValue(args.Fax.Comp.DestinationFaxAddress, out var faxName))
                return;

            var payload = new NetworkPayload()
            {
                { DeviceNetworkConstants.Command, FaxConstants.FaxSendEntityCommand },
                { FaxConstants.FaxEntitySentData, args.Fax.Comp.PaperSlot.Item },
                { FaxConstants.FaxWorkCrossGridData, ent.Comp.CrossGrid }
            };

            _deviceNetwork.QueuePacket(args.Fax, args.Fax.Comp.DestinationFaxAddress, payload);

            var actor = args.Args.Actor;
            if (actor.IsValid())
                _adminLogger.Add(LogType.Action,
                    LogImpact.Low,
                    $"{ToPrettyString(actor):actor} " +
                    $"sent entity {ToPrettyString(sendEntity)} from \"{args.Fax.Comp.FaxName}\" {ToPrettyString(args.Fax):tool} " +
                    $"to \"{faxName}\" ({args.Fax.Comp.DestinationFaxAddress}) ");

            args.Fax.Comp.SendTimeoutRemaining += args.Fax.Comp.SendTimeout;

            _audio.PlayPvs(args.Fax.Comp.SendSound, args.Fax);
        }
    }

    private void OnCanLubedInsert(Entity<FaxSlipComponent> ent, ref CanLubedInsertEvent args)
    {
        args.CanInsert |= ent.Comp.LubedChance != null && HasComp<FaxMachineComponent>(args.Into.Owner);
    }
}
