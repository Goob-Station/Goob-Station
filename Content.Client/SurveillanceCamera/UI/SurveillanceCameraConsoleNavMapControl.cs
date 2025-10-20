// Goobstation Start
// I really want to put this in the goobstation namespace
// but it breaks references in the xaml and as a result the the xaml.cs
using Content.Client.Pinpointer.UI;

namespace Content.Client.SurveillanceCamera.UI
{
    public sealed partial class SurveillanceCameraConsoleNavMapControl : NavMapControl
    {

        public SurveillanceCameraConsoleNavMapControl() : base()
        {
            // Set colors
            TileColor = new Color(30, 57, 67);
            WallColor = new Color(192, 192, 192);
            BackgroundColor = Color.FromSrgb(TileColor.WithAlpha(BackgroundOpacity));
        }

        protected override void UpdateNavMap()
        {
            base.UpdateNavMap();
        }
    }
}
// Goobstation End
