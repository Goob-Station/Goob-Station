using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Goobstation.Client.GPS;

public sealed partial class GpsListEntry : Button
{
    public GpsListEntry()
    {
        RobustXamlLoader.Load(this);
    }
}
