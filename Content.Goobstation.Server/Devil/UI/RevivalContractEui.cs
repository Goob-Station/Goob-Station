// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.Contract.Revival;
using Content.Goobstation.Shared.Devil;
using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared.Mind;

namespace Content.Goobstation.Server.Devil.UI;

public sealed class RevivalContractEui(MindComponent mind, SharedMindSystem mindSystem, PendingRevivalContractSystem revivalContract) : BaseEui
{
    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not RevivalContractMessage { Accepted: true })
        {
            Close();
            return;
        }

        mindSystem.UnVisit(mind.Session);
        revivalContract.TryReviveAndTransferSoul(mind.Session?.AttachedEntity);

        Close();
    }
}
