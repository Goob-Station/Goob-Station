// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.IoC;

namespace Content.Goobstation.Shared.IoC;

internal static class SharedGoobContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;
    }
}