// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Common.JoinQueue;
using Content.Goidastation.Server.JoinQueue;
using Content.Goidastation.Server.Redial;
using Robust.Shared.IoC;

namespace Content.Goidastation.Server.IoC;

internal static class ServerGoidaContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;

        instance.Register<RedialManager>();
        instance.Register<IJoinQueueManager, JoinQueueManager>();
    }
}
