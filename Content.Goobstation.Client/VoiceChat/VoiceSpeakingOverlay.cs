using System.Numerics;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceSpeakingOverlay : Overlay
{
    private const float Pixel = 1f / EyeManager.PixelsPerMeter;
    private const float MinVisibility = 0.01f;
    private static readonly Vector2 Origin = new Vector2(7f, 12f) * Pixel;
    private static readonly Vector2 BoxSize = new Vector2(11f, 9f) * Pixel;

    private readonly IEntityManager _entityManager;
    private readonly VoiceChatSystem _voice;
    private readonly SharedTransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public VoiceSpeakingOverlay(IEntityManager entityManager, VoiceChatSystem voice)
    {
        _entityManager = entityManager;
        _voice = voice;
        _transform = entityManager.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = Matrix3Helpers.CreateRotation(-(args.Viewport.Eye?.Rotation ?? Angle.Zero));

        foreach (var stream in _voice.Streams.Values)
        {
            if (stream.Route == VoiceRoute.Radio || stream.Activity <= 0f)
                continue;

            var visibility = stream.Global ? 1f : stream.Audibility;
            if (visibility < MinVisibility ||
                !_entityManager.TryGetEntity(stream.Source, out var uid) ||
                !_entityManager.TryGetComponent<TransformComponent>(uid, out var xform) ||
                xform.MapID != args.MapId)
            {
                continue;
            }

            var position = _transform.GetWorldPosition(xform);
            if (!args.WorldAABB.Contains(position))
                continue;

            var alpha = stream.Activity * (0.3f + 0.7f * visibility);
            var color = _voice.GetRouteColor(stream).WithAlpha(alpha);
            var levels = stream.Levels;

            handle.SetTransform(Matrix3x2.Multiply(rotation, Matrix3Helpers.CreateTranslation(position)));
            handle.DrawRect(new Box2(Origin, Origin + BoxSize), Color.Black.WithAlpha(0.6f * alpha));
            DrawBar(handle, 0, levels.Low, color);
            DrawBar(handle, 1, levels.Mid, color);
            DrawBar(handle, 2, levels.High, color);
        }

        handle.SetTransform(Matrix3x2.Identity);
    }

    private static void DrawBar(DrawingHandleWorld handle, int index, float level, Color color)
    {
        var height = 1f + 6f * level;
        var bottom = Origin + new Vector2(2f + index * 3f, 1f) * Pixel;
        handle.DrawRect(new Box2(bottom, bottom + new Vector2(2f, height) * Pixel), color);
    }
}
