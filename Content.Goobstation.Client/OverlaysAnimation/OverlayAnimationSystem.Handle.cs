using System.Numerics;
using Content.Goobstation.Shared.OverlaysAnimation;
using Content.Goobstation.Shared.OverlaysAnimation.Animations;
using Content.Goobstation.Shared.OverlaysAnimation.Components;

namespace Content.Goobstation.Client.OverlaysAnimation;

public sealed partial class OverlayAnimationSystem
{
    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (!AnimationsEnabled)
            return;

        // Handle calculation for all animations.
        var query = EntityQueryEnumerator<OverlayAnimationComponent>();
        while (query.MoveNext(out var animationComp))
        {
            animationComp.TimePos += frameTime;

            foreach (var animation in animationComp.Animations)
            {
                // Process the animation only in the specified timing
                if (animation.StartDelay < animationComp.TimePos
                    || animation.StartDelay + animation.Duration > animationComp.TimePos)
                    continue;

                switch (animation)
                {
                    case ColorOverlayAnimation colorAnimation:
                        CalculateColorAnimation(colorAnimation, animationComp, frameTime);
                        break;
                    case FadeOverlayAnimation fadeAnimation:
                        CalculateFadeAnimation(fadeAnimation, animationComp, frameTime);
                        break;
                    case ScaleOverlayAnimation scaleAnimation:
                        CalculateScaleAnimation(scaleAnimation, animationComp, frameTime);
                        break;
                    case TransformOverlayAnimation xformAnimation:
                        CalculateXformAnimation(xformAnimation, animationComp, frameTime);
                        break;
                }
            }
        }
    }

    private void CalculateColorAnimation(ColorOverlayAnimation animation, OverlayAnimationComponent animationComp, float frameTime)
    {
        var timePosition = (animationComp.TimePos - animation.StartDelay) / animation.Duration;
        switch (animation.AnimationType)
        {
            case AnimationType.Instant:
                animationComp.Color = animation.EndColor;
                break;
            case AnimationType.Linear:
                // This is shitcode but it SHOULD work
                var changeColor = new Robust.Shared.Maths.Vector3(
                    animation.StartColor.R - animation.EndColor.R,
                    animation.StartColor.G - animation.EndColor.G,
                    animation.StartColor.B - animation.EndColor.B);

                var colorLinearChange = changeColor * frameTime / animation.Duration;

                animationComp.Color = new Color(
                    animationComp.Color.R + colorLinearChange.X,
                    animationComp.Color.G + colorLinearChange.Y,
                    animationComp.Color.B +colorLinearChange.Z);
                break;
            case AnimationType.Exponential:
                // TODO
                animationComp.Color = animation.EndColor;
                break;
            case AnimationType.Sinus:
                // TODO
                animationComp.Color = animation.EndColor;
                break;
            case AnimationType.Cosinus:
                // TODO
                animationComp.Color = animation.EndColor;
                break;
            default:
                return;
        }
    }

    private void CalculateFadeAnimation(FadeOverlayAnimation animation, OverlayAnimationComponent animationComp, float frameTime)
    {
        var timePosition = (animationComp.TimePos - animation.StartDelay) / animation.Duration;
        switch (animation.AnimationType)
        {
            case AnimationType.Instant:
                animationComp.Opacity = animation.EndOpacity;
                break;
            case AnimationType.Linear:
                var totalChange = animation.StartOpacity - animation.EndOpacity;
                var opacityChange = totalChange * frameTime / animation.Duration;
                animationComp.Opacity += opacityChange;
                break;
            case AnimationType.Exponential:
                var opacityPow = InterpolateExponential(animation.StartOpacity, animation.EndOpacity, timePosition, animation.ExponentSpeed);
                animationComp.Opacity = opacityPow;
                break;
            case AnimationType.Sinus:
                var opacitySin = InterpolateSin(animation.StartOpacity, animation.EndOpacity, timePosition);
                animationComp.Opacity = opacitySin;
                break;
            case AnimationType.Cosinus:
                var opacityCos = InterpolateCos(animation.StartOpacity, animation.EndOpacity, timePosition);
                animationComp.Opacity = opacityCos;
                break;
            default:
                return;
        }
    }

    private void CalculateScaleAnimation(ScaleOverlayAnimation animation, OverlayAnimationComponent animationComp, float frameTime)
    {
        var timePosition = (animationComp.TimePos - animation.StartDelay) / animation.Duration;
        switch (animation.AnimationType)
        {
            case AnimationType.Instant:
                animationComp.Scale = animation.EndScale;
                break;
            case AnimationType.Linear:
                var totalChange = animation.StartScale - animation.EndScale;
                var scaleChange = totalChange * frameTime / animation.Duration;
                animationComp.Scale += scaleChange;
                break;
            case AnimationType.Exponential:
                var scalePow = InterpolateExponential(animation.StartScale, animation.EndScale, timePosition, animation.ExponentSpeed);
                animationComp.Scale = scalePow;
                break;
            case AnimationType.Sinus:
                var scaleSin = InterpolateSin(animation.StartScale, animation.EndScale, timePosition);
                animationComp.Scale = scaleSin;
                break;
            case AnimationType.Cosinus:
                var scaleCos = InterpolateCos(animation.StartScale, animation.EndScale, timePosition);
                animationComp.Scale = scaleCos;
                break;
            default:
                return;
        }
    }

    private void CalculateXformAnimation(TransformOverlayAnimation animation, OverlayAnimationComponent animationComp, float frameTime)
    {
        var timePosition = (animationComp.TimePos - animation.StartDelay) / animation.Duration;
        switch (animation.AnimationType)
        {
            case AnimationType.Instant:
                animationComp.Position = animation.EndPos;
                break;
            case AnimationType.Linear:
                var distanceVec = animation.StartPos - animation.EndPos;
                var moveLinear = distanceVec * frameTime / animation.Duration;
                animationComp.Position += moveLinear;
                break;
            case AnimationType.Exponential:
                var movePowX = InterpolateExponential(animation.StartPos.X, animation.EndPos.X, timePosition, animation.ExponentSpeed);
                var movePowY = InterpolateExponential(animation.StartPos.Y, animation.EndPos.Y, timePosition, animation.ExponentSpeed);
                animationComp.Position = new Vector2(movePowX, movePowY);
                break;
            case AnimationType.Sinus:
                var moveSinX = InterpolateSin(animation.StartPos.X, animation.EndPos.X, timePosition);
                var moveSinY = InterpolateSin(animation.StartPos.Y, animation.EndPos.Y, timePosition);
                animationComp.Position = new Vector2(moveSinX, moveSinY);
                break;
            case AnimationType.Cosinus:
                var moveCosX = InterpolateCos(animation.StartPos.X, animation.EndPos.X, timePosition);
                var moveCosY = InterpolateCos(animation.StartPos.Y, animation.EndPos.Y, timePosition);
                animationComp.Position = new Vector2(moveCosX, moveCosY);
                break;
            default:
                return;
        }
    }

    private static float InterpolateExponential(float startValue, float endValue, float timePosition, float? power = null)
    {
        power ??= 2;
        timePosition = Math.Clamp(timePosition, 0f, 1f);
        return startValue + (endValue - startValue) * MathF.Pow(timePosition, power.Value);
    }

    private static float InterpolateSin(float startValue, float endValue, float timePosition)
    {
        timePosition = Math.Clamp(timePosition, 0f, 1f);
        var sinInput = timePosition * MathF.PI / 2;
        return startValue + (endValue - startValue) * MathF.Sin(sinInput);
    }

    private static float InterpolateCos(float startValue, float endValue, float timePosition)
    {
        timePosition = Math.Clamp(timePosition, 0f, 1f);
        var cosInput = timePosition * MathF.PI / 2;
        return endValue + (startValue - endValue) * MathF.Cos(cosInput);
    }
}
