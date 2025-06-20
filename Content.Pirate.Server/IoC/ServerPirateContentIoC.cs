// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Pirate.Server.Pacification.Managers;
using Robust.Shared.IoC;

namespace Content.Pirate.Server.IoC;

internal static class ServerPirateContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;

        instance.Register<PacifyManager>();
    }
}
