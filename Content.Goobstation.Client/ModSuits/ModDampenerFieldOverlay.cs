using System.Numerics;
using Content.Goobstation.Shared.ModSuits.Modules;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModDampenerFieldOverlay : Overlay
{
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "ModDampenerField";
    private static readonly Color FieldColor = Color.FromHex("#7fd4ffff");

    private readonly SharedTransformSystem _transform;
    private readonly Dictionary<EntityUid, ShaderInstance> _shaders = new();
    private readonly HashSet<EntityUid> _seen = new();
    private readonly List<EntityUid> _stale = new();

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowEntities;

    public ModDampenerFieldOverlay()
    {
        IoCManager.InjectDependencies(this);
        _transform = _entityManager.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var query = _entityManager.EntityQueryEnumerator<ModDampenerFieldComponent, TransformComponent>();
        _seen.Clear();

        while (query.MoveNext(out var uid, out var field, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var center = _transform.GetWorldPosition(xform);
            var bounds = Box2.CenteredAround(center, new Vector2(field.Radius * 2f));
            if (!bounds.Intersects(args.WorldAABB))
                continue;

            _seen.Add(uid);
            if (!_shaders.TryGetValue(uid, out var shader))
            {
                shader = _prototypeManager.Index(Shader).InstanceUnique();
                shader.SetParameter("fieldColor", FieldColor);
                shader.SetParameter("seed", uid.Id * 0.37f % 10f);
                _shaders[uid] = shader;
            }

            shader.SetParameter("pixels", field.Radius * 2f * EyeManager.PixelsPerMeter);
            handle.UseShader(shader);
            handle.DrawTextureRect(Texture.White, bounds);
        }

        handle.UseShader(null);

        _stale.Clear();
        foreach (var uid in _shaders.Keys)
        {
            if (!_seen.Contains(uid))
                _stale.Add(uid);
        }

        foreach (var uid in _stale)
        {
            _shaders[uid].Dispose();
            _shaders.Remove(uid);
        }
    }
}
