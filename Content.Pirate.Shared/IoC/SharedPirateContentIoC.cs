// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.IoC;

namespace Content.Pirate.Shared.IoC;

internal static class SharedPirateContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;
    }
}
