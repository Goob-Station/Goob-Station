using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Cinematic;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Full-screen pressure aura.
/// </summary>
public sealed class CinematicPressureOverlay : Overlay
{
    private const float MinStrength = 0.005f;

    [Dependency] private readonly IEntityManager _entityManager = default!;

    public ShaderInstance? Shader;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public CinematicPressureOverlay()
    {
        IoCManager.InjectDependencies(this);

        // Above the letterbox bars.
        ZIndex = 201;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (Shader == null || !TryGetStrongest(out _))
            return false;

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (Shader is not { } shader || !TryGetStrongest(out var pressure))
            return;

        shader.SetParameter("strength", pressure.Current);
        shader.SetParameter("flowSpeed", pressure.FlowSpeed);
        shader.SetParameter("tint", pressure.Color);
        shader.SetParameter("shock", pressure.Shock);

        var handle = args.ScreenHandle;
        handle.UseShader(shader);
        handle.DrawRect(args.ViewportBounds, Color.White);
        handle.UseShader(null);
    }

    private bool TryGetStrongest([NotNullWhen(true)] out CinematicPressureComponent? pressure)
    {
        pressure = null;
        var strongest = MinStrength;

        var query = _entityManager.EntityQueryEnumerator<CinematicPressureComponent>();
        while (query.MoveNext(out var comp))
        {
            if (comp.Current <= strongest)
                continue;

            strongest = comp.Current;
            pressure = comp;
        }

        return pressure != null;
    }
}
