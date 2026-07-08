// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.JoinQueue;
using Content.Goobstation.Client.MisandryBox;
using Content.Goobstation.Client.Polls;
using Content.Goobstation.Client.Redial;
using Content.Goobstation.Client.ServerCurrency;
using Content.Goobstation.Client.Voice;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.MisandryBox.JumpScare;
using Robust.Shared.IoC;

namespace Content.Goobstation.Client.IoC;

internal static class ContentGoobClientIoC
{
    internal static void Register()
    {
        var collection = IoCManager.Instance!;

        collection.Register<RedialManager>();
        collection.Register<PollManager>();
        collection.Register<IVoiceChatManager, VoiceChatClientManager>();
        collection.Register<JoinQueueManager>();
        collection.Register<IFullScreenImageJumpscare, ClientFullScreenImageJumpscare>();
        collection.Register<ICommonCurrencyManager, ClientCurrencyManager>();
    }
}
