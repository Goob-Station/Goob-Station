using Content.Shared.StatusEffectNew;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Client.Insanity;

public sealed class InsanityOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "Insanity";

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly ShaderInstance _insanityShader;
    private float _time;

    // Tentacle parameters
    private const float Radius = 200f;
    private const float EdgeSoftness = 50f;
    private const float WaveAmplitude = 10f;   // Pixels
    private const float WaveSpeed = 2f;        // Rotation speed
    private const float NumTentacles = 6f;     // Number of tentacle arms
    private const float BaseAlpha = 0.6f;      // Darkness outside circle

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public InsanityOverlay()
    {
        IoCManager.InjectDependencies(this);
        _insanityShader = _prototypeManager.Index(Shader).InstanceUnique();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        // Animate tentacles
        _time += (float) args.DeltaSeconds;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return _playerManager.LocalEntity != null;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;

        // Compute center and size manually
        var min = args.WorldBounds.TopLeft;     // Vector2
        var max = args.WorldBounds.BottomRight; // Vector2
        var center = (min + max) / 2f;
        var size = max - min;

        _insanityShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _insanityShader.SetParameter("CircleCenter", center);
        _insanityShader.SetParameter("Radius", Radius);
        _insanityShader.SetParameter("EdgeSoftness", EdgeSoftness);
        _insanityShader.SetParameter("Time", _time);
        _insanityShader.SetParameter("WaveAmplitude", WaveAmplitude);
        _insanityShader.SetParameter("WaveSpeed", WaveSpeed);
        _insanityShader.SetParameter("NumTentacles", NumTentacles);
        _insanityShader.SetParameter("BaseAlpha", BaseAlpha);

        handle.UseShader(_insanityShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
