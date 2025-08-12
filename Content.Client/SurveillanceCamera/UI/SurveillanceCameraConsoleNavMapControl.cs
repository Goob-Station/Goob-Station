using Content.Client.Pinpointer.UI;
using Robust.Shared.Map.Components;

namespace Content.Client.SurveillanceCamera.UI
{
    public sealed partial class SurveillanceCameraConsoleNavMapControl : NavMapControl
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        private Dictionary<Color, Color> _sRGBLookUp = new Dictionary<Color, Color>();
        private MapGridComponent? _grid;

        public SurveillanceCameraConsoleNavMapControl() : base()
        {
            // Set colors
            TileColor = new Color(30, 57, 67);
            WallColor = new Color(102, 164, 217);
            BackgroundColor = Color.FromSrgb(TileColor.WithAlpha(BackgroundOpacity));
            //PostWallDrawingAction += DrawAllCameras;
        }

        protected override void UpdateNavMap()
        {
            base.UpdateNavMap();
        }
    }
}
