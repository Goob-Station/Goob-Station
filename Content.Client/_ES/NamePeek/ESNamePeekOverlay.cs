using System.Numerics;
using Content.Client.Chat.UI;
using Content.Client.Examine;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Light;
using Robust.Shared.Prototypes;

namespace Content.Client._ES.NamePeek;

public sealed class ESNamePeekOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";
    private const int FontSize = 12;
    private const float MinLightLevel = 0.1f;

    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private ILocalizationManager _loc = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IResourceCache _cache = default!;

    private readonly ESNamePeekSystem _namePeek;
    private readonly ExamineSystem _examine;
    private readonly EntityLookupSystem _lookup;
    private readonly LightLevelSystem _lightLevel;
    private readonly SharedTransformSystem _transform;
    private readonly SpriteSystem _sprite;
    private readonly ShaderInstance _shader;
    private readonly Font _font;
    private readonly Font _smallFont;

    private readonly HashSet<Entity<MobStateComponent>> _nearbyEntities = new();

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public ESNamePeekOverlay(ESNamePeekSystem namePeek)
    {
        IoCManager.InjectDependencies(this);

        _namePeek = namePeek;
        _examine = _entityManager.System<ExamineSystem>();
        _lookup = _entityManager.System<EntityLookupSystem>();
        _lightLevel = _entityManager.System<LightLevelSystem>();
        _transform = _entityManager.System<SharedTransformSystem>();
        _sprite = _entityManager.System<SpriteSystem>();

        _shader = _prototypeManager.Index(UnshadedShader).Instance();
        _font = new VectorFont(_cache.GetResource<FontResource>(RunechatSheetlet.FontPath), FontSize);
        _smallFont = new VectorFont(_cache.GetResource<FontResource>(RunechatSheetlet.WhisperFontPath), FontSize);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null || !_namePeek.Visible)
            return false;

        return !_entityManager.TryGetComponent<MobStateComponent>(_playerManager.LocalEntity, out var mobState)
               || mobState.CurrentState != MobState.Critical;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null
            || _playerManager.LocalEntity is not { } playerEnt
            || args.Viewport.Eye is not { } eye)
            return;

        var scale = (args.ViewportControl as Control)?.UIScale ?? 0f;
        if (scale <= 0f)
            return;

        args.DrawingHandle.SetTransform(Matrix3x2.Identity);
        args.DrawingHandle.UseShader(_shader);

        var handle = args.ScreenHandle;
        var matrix = args.ViewportControl.GetWorldToScreenMatrix();
        var outline = new TextOutline(FontSize * 96f / 72f / 8f, Color.Black);

        _nearbyEntities.Clear();
        _lookup.GetEntitiesIntersecting(args.MapId, args.WorldAABB, _nearbyEntities, LookupFlags.Uncontained);

        foreach (var ent in _nearbyEntities)
        {
            if (ent.Owner == playerEnt)
                continue;

            if (!_entityManager.TryGetComponent<TransformComponent>(ent, out var xform)
                || !_entityManager.TryGetComponent<SpriteComponent>(ent, out var sprite)
                || !sprite.Visible
                || sprite.Color.A <= 0f)
                continue;

            var mapPos = _transform.GetMapCoordinates((ent, xform));

            if (eye.DrawLight && eye.DrawFov
                && _lightLevel.TryCalculateLightLevel(mapPos, out var lightLevel)
                && lightLevel <= MinLightLevel)
                continue;

            if (eye.DrawFov && !_examine.InRangeUnOccluded(playerEnt, ent))
                continue;

            var text = Identity.Name(ent, _entityManager, playerEnt);
            var dimensions = handle.GetDimensions(_font, text, scale);

            var (worldPos, worldRot) = _transform.GetWorldPositionRotation(xform);
            var bounds = _sprite.CalculateBounds((ent, sprite), worldPos, worldRot, eye.Rotation);
            var offset = (-eye.Rotation).ToWorldVec() * -bounds.Box.Enlarged(0.15f).Extents.Y;

            var pos = Vector2.Transform(worldPos - offset, matrix);
            var color = RunechatSpeechBubble.GetNameColor(text);

            handle.DrawString(_font, (pos - dimensions / 2f).Rounded(), text, scale, color, outline);

            if (!_entityManager.TryGetComponent<HumanoidAppearanceComponent>(ent, out var humanoid))
                continue;

            var pronouns = _loc.GetString(humanoid.Gender switch
            {
                Gender.Male => "humanoid-profile-editor-pronouns-male-text",
                Gender.Female => "humanoid-profile-editor-pronouns-female-text",
                Gender.Neuter => "humanoid-profile-editor-pronouns-neuter-text",
                _ => "humanoid-profile-editor-pronouns-epicene-text",
            });
            var pronounDimensions = handle.GetDimensions(_smallFont, pronouns, scale);
            var pronounPos = pos - pronounDimensions / 2f + new Vector2(0f, dimensions.Y);

            handle.DrawString(_smallFont, pronounPos.Rounded(), pronouns, scale, color, outline);
        }

        args.DrawingHandle.UseShader(null);
    }
}
