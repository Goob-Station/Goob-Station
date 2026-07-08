// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;

namespace Content.Client._DV.CosmicCult.UI;

public sealed class CosmicDeconvertedEui : BaseEui
{
    private readonly CosmicDeconvertedMenu _menu;

    public CosmicDeconvertedEui()
    {
        _menu = new CosmicDeconvertedMenu();
    }

    public override void Opened()
    {
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}
