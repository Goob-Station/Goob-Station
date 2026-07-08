// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client.Shuttles.UI;

public sealed partial class RadarConsoleWindow
{
    public void SetConsole(EntityUid consoleEntity)
    {
        RadarScreen.SetConsole(consoleEntity);
    }
}
