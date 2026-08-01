// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;

namespace Content.Client._DV.CosmicCult.UI;

public sealed class CosmicConvertedEui : BaseEui
{
    private readonly CosmicConvertedMenu _menu;

    public CosmicConvertedEui() => _menu = new CosmicConvertedMenu();

    public override void Opened() => _menu.OpenCentered();

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}
