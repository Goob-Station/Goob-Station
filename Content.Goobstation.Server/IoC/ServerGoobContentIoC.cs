// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Sara Aldrete's Top Guy <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.JoinQueue;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Server.JoinQueue;
using Content.Goobstation.Server.MisandryBox.JumpScare;
using Content.Goobstation.Server.Polls;
using Content.Goobstation.Server.Redial;
using Content.Goobstation.Server.ServerCurrency;
using Content.Goobstation.Server.Voice;
using Content.Goobstation.Shared.MisandryBox.JumpScare;

namespace Content.Goobstation.Server.IoC;

internal static class ServerGoobContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;

        instance.Register<RedialManager>();
        instance.Register<PollManager>();
        instance.Register<IVoiceChatServerManager, VoiceChatServerManager>();
        instance.Register<IJoinQueueManager, JoinQueueManager>();
        instance.Register<IFullScreenImageJumpscare, ServerFullScreenImageJumpscare>();
        instance.Register<ICommonCurrencyManager, ServerCurrencyManager>();
    }
}
