using Content.Goobstation.Shared.Cinematic;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// black cinematic bars that appear on the top / bottom of the screen.
/// </summary>
public sealed partial class CinematicLetterboxOverlay : Overlay
{
    [Dependency] IEntityManager _entityManager = default!;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public CinematicLetterboxOverlay()
    {
        IoCManager.InjectDependencies(this);

        ZIndex = 200;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var query = _entityManager.EntityQueryEnumerator<CinematicLetterboxComponent>();
        while (query.MoveNext(out var letterbox))
        {
            if (letterbox.Strength > 0f && letterbox.Fraction > 0f)
                return base.BeforeDraw(in args);
        }

        return false;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var bounds = args.ViewportBounds;

        var query = _entityManager.EntityQueryEnumerator<CinematicLetterboxComponent>();
        while (query.MoveNext(out var letterbox))
        {
            if (letterbox.Strength <= 0f || letterbox.Fraction <= 0f)
                continue;

            var height = bounds.Height * letterbox.Fraction * letterbox.Strength;
            var feather = height * letterbox.Feather;
            float left = bounds.Left;
            float right = bounds.Right;

            handle.DrawRect(new UIBox2(left, bounds.Top, right, bounds.Top + height), letterbox.Color);
            handle.DrawRect(new UIBox2(left, bounds.Bottom - height, right, bounds.Bottom), letterbox.Color);

            var featherColor = letterbox.Color.WithAlpha(letterbox.Color.A * 0.4f);
            handle.DrawRect(
                new UIBox2(left, bounds.Top + height, right, bounds.Top + height + feather),
                featherColor);
            handle.DrawRect(
                new UIBox2(left, bounds.Bottom - height - feather, right, bounds.Bottom - height),
                featherColor);
        }
    }
}
