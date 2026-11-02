using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._Funkystation.Clothing.Systems;

public sealed class GoggleShaderOverlay(IPrototypeManager prototype) : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public readonly List<(string Shader, Color Color)> ActiveShaders = new();

    public bool ReducedMotion;

    private readonly Dictionary<string, ShaderInstance> _instances = new();

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return ActiveShaders.Count > 0;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var worldHandle = args.WorldHandle;
        var worldBounds = args.WorldBounds;

        foreach (var (shaderId, color) in ActiveShaders)
        {
            if (!_instances.TryGetValue(shaderId, out var instance))
            {
                if (!prototype.TryIndex<ShaderPrototype>(shaderId, out var proto))
                    continue;

                instance = proto.InstanceUnique();
                _instances[shaderId] = instance;
            }

            instance.SetParameter("color", color);
            instance.SetParameter("SCREEN_TEXTURE", ScreenTexture);

            instance.SetParameter("reducedMotion", ReducedMotion ? 1.0f : 0.0f);

            worldHandle.UseShader(instance);
            worldHandle.DrawRect(worldBounds, Color.White);
        }

        worldHandle.UseShader(null);
    }
}
