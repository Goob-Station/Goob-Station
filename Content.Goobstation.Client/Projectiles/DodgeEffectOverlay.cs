using Content.Goobstation.Shared.Projectiles;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Client.Projectiles;

public sealed class DodgeEffectOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> ShaderProto = "Dodge";

    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private SharedTransformSystem? _xformSystem;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ShaderInstance _shader;
    private const float EffectWorldSize = 1.75f;

    public readonly Dictionary<EntityUid, (TimeSpan Time, float Seed)> Effects = new();

    public void AddEffect(EntityUid uid, TimeSpan time)
    {
        Effects[uid] = (time, _random.NextFloat() * 1000f);
    }

    public void RemoveEffect(EntityUid uid)
    {
        Effects.Remove(uid);
    }

    public DodgeEffectOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _protoMan.Index(ShaderProto).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_xformSystem == null && !_entMan.TrySystem(out _xformSystem))
            return false;

        return Effects.Count > 0;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_xformSystem == null)
            return;

        var worldHandle = args.WorldHandle;
        var now = _timing.RealTime;

        foreach (var (uid, effect) in Effects)
        {
            if (!_entMan.TryGetComponent<DodgeEffectComponent>(uid, out var dodge))
                continue;

            if (!_entMan.TryGetComponent<TransformComponent>(uid, out var xform))
                continue;

            if (xform.MapID != args.MapId)
                continue;

            var worldPos = _xformSystem.GetWorldPosition(uid);
            var elapsed = (float) (now - effect.Time).TotalSeconds;
            var progress = Math.Clamp(elapsed / dodge.Duration, 0f, 1f);

            var screenCenter = args.Viewport.WorldToLocal(worldPos);
            screenCenter.Y = args.Viewport.Size.Y - screenCenter.Y;

            var pixelsPerMeter = EyeManager.PixelsPerMeter * args.Viewport.RenderScale;
            var screenSize = new Vector2(EffectWorldSize, EffectWorldSize) * pixelsPerMeter;

            _shader.SetParameter("progress", progress);
            _shader.SetParameter("center", screenCenter);
            _shader.SetParameter("size", screenSize);
            _shader.SetParameter("seed", effect.Seed);

            var worldBox = Box2.CenteredAround(worldPos, new Vector2(EffectWorldSize, EffectWorldSize));

            worldHandle.UseShader(_shader);
            worldHandle.DrawRect(worldBox, Color.White);
            worldHandle.UseShader(null);
        }
    }
}
