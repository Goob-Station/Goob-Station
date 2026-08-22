using System.Numerics;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.StatusEffectNew;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Draws a small vertical progress bar beside every victim the local player is brainwashing as the Idol slasher
/// </summary>
public sealed class LovestruckProgressOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SharedTransformSystem? _xform;
    private StatusEffectsSystem? _statusEffects;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private static readonly Box2 BarBounds = new(new Vector2(0.38f, -0.28f), new Vector2(0.46f, 0.32f));
    private static readonly Color BackgroundColor = new(0.05f, 0.02f, 0.04f, 0.72f);
    private static readonly Color EmptyColor = Color.FromHex("#5C1030");
    private static readonly Color FullColor = Color.FromHex("#FF5C8A");

    public LovestruckProgressOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null
            || _player.LocalEntity is not { } local
            || _xform is null && !_entMan.TrySystem(out _xform)
            || _statusEffects is null && !_entMan.TrySystem(out _statusEffects))
            return;

        var handle = args.WorldHandle;
        var eyeRot = args.Viewport.Eye.Rotation;
        var rotationMatrix = Matrix3Helpers.CreateRotation(-eyeRot);
        var bounds = args.WorldAABB.Enlarged(1f);
        var drawn = false;

        var query = _entMan.AllEntityQueryEnumerator<LovestruckComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (comp.Idol != local
                || xform.MapID != args.MapId
                || _statusEffects.HasStatusEffect(uid, comp.LovestruckStatusEffect))
                continue;

            var progress = Math.Clamp(comp.Accumulated / Math.Max(comp.ConversionThreshold, 1f), 0f, 1f);
            if (progress <= 0f)
                continue;

            var worldPos = _xform.GetWorldPosition(uid);
            if (!bounds.Contains(worldPos))
                continue;

            handle.SetTransform(Matrix3x2.Multiply(rotationMatrix, Matrix3Helpers.CreateTranslation(worldPos)));
            handle.DrawRect(BarBounds, BackgroundColor);

            var inner = new Box2(BarBounds.BottomLeft + new Vector2(0.015f, 0.015f),
                                 BarBounds.TopRight - new Vector2(0.015f, 0.015f));
            var fill = inner with { Top = inner.Bottom + inner.Height * progress };
            handle.DrawRect(fill, Color.InterpolateBetween(EmptyColor, FullColor, progress));

            drawn = true;
        }

        if (drawn)
            handle.SetTransform(Matrix3x2.Identity);
    }
}
