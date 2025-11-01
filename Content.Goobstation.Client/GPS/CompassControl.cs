using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.GPS;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;

namespace Content.Goobstation.Client.GPS;

public sealed class CompassControl : Control
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;

    private GpsBoundUserInterfaceState? _lastState;
    private MapCoordinates? _gpsCoordinates;

    private const int GridLines = 10;
    private const float DotRadius = 3f;
    private const float SelectedDotRadius = 6f;
    private const float DistressDotRadius = 5f;

    public CompassControl()
    {
        IoCManager.InjectDependencies(this);
    }

    public void UpdateState(GpsBoundUserInterfaceState state, MapCoordinates? gpsCoords)
    {
        _lastState = state;
        _gpsCoordinates = gpsCoords;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var gridSize = Math.Min(PixelSize.X, PixelSize.Y) * 0.9f;
        var box = UIBox2.FromDimensions(Vector2.Zero, PixelSize);
        var center = box.Center;
        var halfSize = gridSize / 2;

        var gridTopLeft = center - new Vector2(halfSize, halfSize);
        var gridBottomRight = center + new Vector2(halfSize, halfSize);

        // Draw background
        handle.DrawRect(new UIBox2(gridTopLeft, gridBottomRight), Color.Black.WithAlpha(0.5f));

        // Draw grid lines
        var cellStep = gridSize / GridLines;
        for (var i = 0; i <= GridLines; i++)
        {
            var verticalStart = gridTopLeft + new Vector2(i * cellStep, 0);
            var verticalEnd = verticalStart + new Vector2(0, gridSize);
            handle.DrawLine(verticalStart, verticalEnd, Color.White.WithAlpha(0.2f));

            var horizontalStart = gridTopLeft + new Vector2(0, i * cellStep);
            var horizontalEnd = horizontalStart + new Vector2(gridSize, 0);
            handle.DrawLine(horizontalStart, horizontalEnd, Color.White.WithAlpha(0.2f));
        }

        handle.DrawRect(new UIBox2(gridTopLeft, gridBottomRight), Color.White, false);

        if (_lastState == null || _gpsCoordinates == null)
            return;

        handle.DrawCircle(center, DotRadius, Color.Cyan);

        var gridScale = gridSize / GridLines;

        foreach (var entry in _lastState.GpsEntries)
        {
            if (entry.Coordinates.MapId != _gpsCoordinates.Value.MapId)
                continue;

            var worldVec = entry.Coordinates.Position - _gpsCoordinates.Value.Position;

            var screenVec = _eyeManager.CurrentEye.Rotation.RotateVec(worldVec);

            screenVec.Y = -screenVec.Y;

            var gridPos = center + screenVec * gridScale;

            var clampedPos = gridPos;
            clampedPos.X = Math.Clamp(clampedPos.X, gridTopLeft.X, gridBottomRight.X);
            clampedPos.Y = Math.Clamp(clampedPos.Y, gridTopLeft.Y, gridBottomRight.Y);

            var isSelected = _lastState.TrackedEntity == entry.NetEntity;
            var radius = isSelected
                ? SelectedDotRadius
                : entry.IsDistress
                    ? DistressDotRadius
                    : DotRadius;
            var color = isSelected
                ? Color.Cyan
                : entry.IsDistress
                    ? Color.Pink
                    : Color.Red;

            handle.DrawCircle(clampedPos, radius, color);
        }
    }
}
