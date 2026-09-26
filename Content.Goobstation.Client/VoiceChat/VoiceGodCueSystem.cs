using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceGodCueSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly VoiceChatSystem _voice = default!;

    private const float RiseTime = 0.15f;
    private const float FallTime = 1.2f;
    private const float CueHold = 0.6f;
    private const float PeakIntensity = 0.55f;
    private const float HoldLevel = 0.6f;

    private VoiceGodCueOverlay _overlay = default!;
    private float _cue;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new VoiceGodCueOverlay();
        _overlays.AddOverlay(_overlay);
        SubscribeNetworkEvent<VoiceGodCueEvent>(_ => _cue = CueHold);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlays.RemoveOverlay(_overlay);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var activity = 0f;
        var level = 0f;
        foreach (var stream in _voice.Streams.Values)
        {
            if (stream.Route != VoiceRoute.God)
                continue;

            activity = MathF.Max(activity, stream.Activity);
            level = MathF.Max(level, stream.Levels.Overall);
        }

        _cue = MathF.Max(0f, _cue - frameTime);

        var target = PeakIntensity * activity * (HoldLevel + (1f - HoldLevel) * level);
        if (_cue > 0f)
            target = MathF.Max(target, PeakIntensity);

        var current = _overlay.Intensity;
        var step = PeakIntensity * frameTime / (target > current ? RiseTime : FallTime);
        _overlay.Intensity = target > current
            ? MathF.Min(target, current + step)
            : MathF.Max(target, current - step);
    }
}

public sealed class VoiceGodCueOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "VoiceGodGlow";
    private static readonly Color GlowColor = Color.FromHex("#FFE9A8");
    private const float GlowSize = 140f;
    private const float MinIntensity = 0.002f;

    private readonly ShaderInstance _shader;

    public float Intensity;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public override bool RequestScreenTexture => true;

    public VoiceGodCueOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototype.Index(Shader).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return Intensity > MinIntensity;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        _shader.SetParameter("glowColor", GlowColor);
        _shader.SetParameter("intensity", Intensity);
        _shader.SetParameter("glowSize", GlowSize);

        var handle = args.WorldHandle;
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
