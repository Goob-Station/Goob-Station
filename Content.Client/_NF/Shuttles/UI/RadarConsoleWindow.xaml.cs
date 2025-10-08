// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client.Shuttles.UI;

public sealed partial class RadarConsoleWindow
{
    public void SetConsole(EntityUid consoleEntity)
    {
        RadarScreen.SetConsole(consoleEntity);
    }
}
