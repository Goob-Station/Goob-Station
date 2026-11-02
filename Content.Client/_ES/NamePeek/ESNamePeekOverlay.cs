using System.Numerics;
using Content.Client.Chat.UI;
using Content.Client.Examine;
using Content.Client.Silicons.StationAi;
using Content.Goobstation.Common.CCVar;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Flash.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Light;
using Robust.Shared.Prototypes;

namespace Content.Client._ES.NamePeek;

public sealed class ESNamePeekOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";
    private const string RegularFontPath = "/Fonts/NotoSans/NotoSans-Bold.ttf";
    private const string RegularSmallFontPath = "/Fonts/NotoSans/NotoSans-Italic.ttf";
    private const int RegularFontSize = 12;
    private const int RegularSmallFontSize = 10;
    private const float MinLightLevel = 0.1f;
    private const float MinStealthVisibility = 0.5f;

    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private ILocalizationManager _loc = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    private readonly ESNamePeekSystem _namePeek;
    private readonly ExamineSystem _examine;
    private readonly EntityLookupSystem _lookup;
    private readonly LightLevelSystem _lightLevel;
    private readonly SharedTransformSystem _transform;
    private readonly SpriteSystem _sprite;
    private readonly SharedStealthSystem _stealth;
    private readonly StationAiSystem _stationAi;
    private readonly ShaderInstance _shader;

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
        _stealth = _entityManager.System<SharedStealthSystem>();
        _stationAi = _entityManager.System<StationAiSystem>();

        _shader = _prototypeManager.Index(UnshadedShader).Instance();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null || !_namePeek.Visible)
            return false;

        var player = _playerManager.LocalEntity;
        if (_entityManager.HasComponent<FlashedComponent>(player)
            || _entityManager.TryGetComponent<BlindableComponent>(player, out var blindable) && blindable.IsBlind)
            return false;

        return !_entityManager.TryGetComponent<MobStateComponent>(player, out var mobState)
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
        Font font;
        Font smallFont;
        TextOutline outline;
        if (_cfg.GetCVar(GoobCVars.ChatRunechat))
        {
            var size = Math.Clamp(_cfg.GetCVar(GoobCVars.ChatRunechatFontSize), RunechatStyle.MinFontSize, RunechatStyle.MaxFontSize);
            font = RunechatStyle.GetFont(RunechatStyle.FontPath, size);
            smallFont = RunechatStyle.GetFont(RunechatStyle.WhisperFontPath, size);
            outline = new TextOutline(RunechatStyle.GetOutline(size, _cfg.GetCVar(GoobCVars.ChatRunechatOutlineScale)), Color.Black);
        }
        else
        {
            font = RunechatStyle.GetFont(RegularFontPath, RegularFontSize);
            smallFont = RunechatStyle.GetFont(RegularSmallFontPath, RegularSmallFontSize);
            outline = new TextOutline(1f, Color.Black);
        }

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

            if (_entityManager.TryGetComponent<StealthComponent>(ent, out var stealth)
                && _stealth.GetVisibility(ent, stealth) < MinStealthVisibility)
                continue;

            if (!_stationAi.IsVisibleToLocalAi(ent))
                continue;

            var mapPos = _transform.GetMapCoordinates((ent, xform));

            if (eye.DrawLight && eye.DrawFov
                && _lightLevel.TryCalculateLightLevel(mapPos, out var lightLevel)
                && lightLevel <= MinLightLevel)
                continue;

            if (eye.DrawFov && !_examine.InRangeUnOccluded(playerEnt, ent))
                continue;

            var text = Identity.Name(ent, _entityManager, playerEnt);
            var dimensions = handle.GetDimensions(font, text, scale);

            var (worldPos, worldRot) = _transform.GetWorldPositionRotation(xform);
            var bounds = _sprite.CalculateBounds((ent, sprite), worldPos, worldRot, eye.Rotation);
            var offset = (-eye.Rotation).ToWorldVec() * -bounds.Box.Enlarged(0.15f).Extents.Y;

            var pos = Vector2.Transform(worldPos - offset, matrix);
            var color = RunechatSpeechBubble.GetNameColor(text);

            handle.DrawString(font, (pos - dimensions / 2f).Rounded(), text, scale, color, outline);

            if (!_entityManager.TryGetComponent<HumanoidAppearanceComponent>(ent, out var humanoid))
                continue;

            var pronouns = _loc.GetString(humanoid.Gender switch
            {
                Gender.Male => "humanoid-profile-editor-pronouns-male-text",
                Gender.Female => "humanoid-profile-editor-pronouns-female-text",
                Gender.Neuter => "humanoid-profile-editor-pronouns-neuter-text",
                _ => "humanoid-profile-editor-pronouns-epicene-text",
            });
            var pronounDimensions = handle.GetDimensions(smallFont, pronouns, scale);
            var pronounPos = pos - pronounDimensions / 2f + new Vector2(0f, dimensions.Y);

            handle.DrawString(smallFont, pronounPos.Rounded(), pronouns, scale, color, outline);
        }

        args.DrawingHandle.UseShader(null);
    }
}
