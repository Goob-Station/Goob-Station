namespace Content.Client.DoAfter;

public sealed partial class DoAfterOverlay
{
    private static readonly TimeSpan MaxAlphaTime = TimeSpan.FromSeconds(0.3f);
    private static readonly TimeSpan FadeoutAlphaTime = TimeSpan.FromSeconds(0.2f);
    private static readonly TimeSpan MaxYPosTime = TimeSpan.FromSeconds(0.5f);

    private static float GetDoAfterAlpha(TimeSpan elapsed, TimeSpan delay, float maxAlpha)
    {
        if (elapsed >= delay)
            return MathHelper.Lerp(maxAlpha, 0f, (float) Math.Clamp((elapsed - delay) / FadeoutAlphaTime, 0.0, 1.0));

        return MathHelper.Lerp(0f, maxAlpha, (float) Math.Clamp(elapsed / MaxAlphaTime, 0.0, 1.0));
    }

    private static float GetDoAfterYOffset(TimeSpan elapsed, float spriteHeight)
    {
        var yFinished = spriteHeight / 2f;
        var yStart = yFinished / 6f;
        return MathHelper.Lerp(yStart, yFinished, Easings.OutSine((float) Math.Clamp(elapsed / MaxYPosTime, 0.0, 1.0)));
    }
}
