// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Client.JoinQueue;
using Content.Goidastation.Client.Redial;
using Robust.Shared.IoC;

namespace Content.Goidastation.Client.IoC;

internal static class ContentGoidaClientIoC
{
    internal static void Register()
    {
        var collection = IoCManager.Instance!;

        collection.Register<RedialManager>();
        collection.Register<JoinQueueManager>();
    }
}
