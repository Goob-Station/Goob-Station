using System.Numerics;
using Content.Client.UserInterface.Systems;
using Content.Shared._Goobstation.Fishing.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Goobstation.Fishing.Overlays;

public sealed class FishingOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly IPlayerManager _player;
    private readonly SharedTransformSystem _transform;
    private readonly ProgressColorSystem _progressColor;

    private readonly Texture _barTexture;

    // Hardcoded width of the progress bar because it doesn't match the texture.
    private const float StartY = 4;
    private const float EndY = 29;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public FishingOverlay(IEntityManager entManager, IPlayerManager player)
    {
        _entManager = entManager;
        _player = player;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
        _progressColor = _entManager.System<ProgressColorSystem>();
        var sprite = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Interface/Misc/fish_bar.rsi"), "icon");
        _barTexture = _entManager.EntitySysManager.GetEntitySystem<SpriteSystem>().Frame0(sprite);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var rotation = args.Viewport.Eye?.Rotation ?? Angle.Zero;
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        // If you use the display UI scale then need to set max(1f, displayscale) because 0 is valid.
        const float scale = 1f;
        var scaleMatrix = Matrix3Helpers.CreateScale(new Vector2(scale, scale));
        var rotationMatrix = Matrix3Helpers.CreateRotation(-rotation);

        var bounds = args.WorldAABB.Enlarged(5f);
        var localEnt = _player.LocalSession?.AttachedEntity;

        var enumerator = _entManager.AllEntityQueryEnumerator<ActiveFisherComponent, SpriteComponent, TransformComponent>();
        while (enumerator.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId ||
                comp.TotalProgress < 0 ||
                uid != localEnt)
                continue;

            var worldPosition = _transform.GetWorldPosition(xform, xformQuery);
            if (!bounds.Contains(worldPosition))
                continue;

            var worldMatrix = Matrix3Helpers.CreateTranslation(worldPosition);
            var scaledWorld = Matrix3x2.Multiply(scaleMatrix, worldMatrix);
            var matty = Matrix3x2.Multiply(rotationMatrix, scaledWorld);
            handle.SetTransform(matty);

            // Use the sprite itself if we know its bounds. This means short or tall sprites don't get overlapped
            // by the bar.
            float xOffset = sprite.Bounds.Width / 2f + 0.05f;

            // Position above the entity (we've already applied the matrix transform to the entity itself)
            // Offset by the texture size for every do_after we have.
            var position = new Vector2(
                xOffset / scale / EyeManager.PixelsPerMeter * scale,
                -_barTexture.Width / 2f / EyeManager.PixelsPerMeter);

            // Draw the underlying bar texture
            handle.DrawTexture(_barTexture, position);

            var progress = Math.Min(1, comp.TotalProgress);
            var color = GetProgressColor(progress);

            var yProgress = (EndY - StartY) * progress + StartY;
            var box = new Box2(new Vector2(3f, StartY) / EyeManager.PixelsPerMeter, new Vector2(4f, yProgress) / EyeManager.PixelsPerMeter);
            box = box.Translated(position);
            handle.DrawRect(box, color);
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    public Color GetProgressColor(float progress, float alpha = 1f)
    {
        return _progressColor.GetProgressColor(progress).WithAlpha(alpha);
    }
}
