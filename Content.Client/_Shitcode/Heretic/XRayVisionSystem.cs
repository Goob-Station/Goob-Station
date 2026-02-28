using Content.Shared.Heretic.Effects;
using Robust.Client.Graphics;

namespace Content.Client._Shitcode.Heretic;

public sealed class XRayVisionSystem : SharedXRayVisionSystem
{
    [Dependency] private readonly ILightManager _light = default!;

    protected override void DrawLight(bool value)
    {
        base.DrawLight(value);

        _light.DrawLighting = value;
    }
}
