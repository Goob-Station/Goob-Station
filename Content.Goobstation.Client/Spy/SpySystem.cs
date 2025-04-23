using System.Linq;
using Content.Goobstation.Shared.Spy;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Spy;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpySystem : SharedSpySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        //SubscribeNetworkEvent<SpyStartStealEvent>(OnSpyStartStealEvent); so cursed bro
    }

    private void OnSpyStartStealEvent(SpyStartStealEvent ev)
    {
        TrySetShader(GetEntity(ev.Target));
    }

    private bool TrySetShader(EntityUid target, SpriteComponent? sprite = null)
    {
        if (!Resolve(target, ref sprite))
            return false;

        float texHeight = sprite.AllLayers.Max(x => x.PixelSize.Y);

        var instance = _prototype.Index<ShaderPrototype>("Hologram").InstanceUnique();
        var color1 = Color.FromHex("#65b8e2");
        var color2 = Color.FromHex("#3a6981");
        const float alpha = 0.9f;
        const float intensity = 2f;
        const float scrollRate = 2f;

        instance.SetParameter("color1", new Vector3(color1.R, color1.G, color1.B));
        instance.SetParameter("color2", new Vector3(color2.R, color2.G, color2.B));
        instance.SetParameter("alpha", alpha);
        instance.SetParameter("intensity", intensity);
        instance.SetParameter("texHeight", texHeight);
        instance.SetParameter("t", (float) _timing.CurTime.TotalSeconds * scrollRate);

        sprite.PostShader = instance;
        sprite.RaiseShaderEvent = true;

        return true;
    }
}
