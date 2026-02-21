using Content.Shared.FixedPoint;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Timing;

namespace Content.Client._ES.PainFlash;

public sealed class ESPainFlashOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => false;

    private const float MaxPain = 100;

    private float _painAccumulator;

    public void ResetPainAccumulator()
    {
        _painAccumulator = 0;
    }

    public void AddPain(FixedPoint2 inPain)
    {
        _painAccumulator = Math.Clamp(_painAccumulator + inPain.Float(), 5f, 200); // arbitrary number
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_painAccumulator <= 0)
            return;

        _painAccumulator = _painAccumulator - args.DeltaSeconds * 40;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_painAccumulator <= 0)
            return;

        var handle = args.WorldHandle;

        var alpha = Math.Clamp(_painAccumulator / MaxPain, 0, 1);
        var color = Color.Red.WithAlpha(alpha);

        handle.DrawRect(args.WorldBounds, color);
    }
}
