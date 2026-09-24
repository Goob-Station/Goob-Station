using System.Numerics;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.StatusEffectNew;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Slasher.Overlays;

public sealed class CharmedProgressOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly SharedTransformSystem _xform;
    private readonly StatusEffectsSystem _statusEffects;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public CharmedProgressOverlay()
    {
        IoCManager.InjectDependencies(this);

        _xform = _entMan.System<SharedTransformSystem>();
        _statusEffects = _entMan.System<StatusEffectsSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye is not { } eye || _player.LocalEntity is not { } local)
            return;

        var handle = args.WorldHandle;
        var rotationMatrix = Matrix3Helpers.CreateRotation(-eye.Rotation);
        var bounds = args.WorldAABB.Enlarged(1f);

        var query = _entMan.AllEntityQueryEnumerator<CharmedComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (comp.Idol != local
                || xform.MapID != args.MapId
                || _statusEffects.HasStatusEffect(uid, comp.CharmedStatusEffect))
                continue;

            var progress = Math.Clamp(comp.Accumulated / Math.Max(comp.ConversionThreshold, 1f), 0f, 1f);
            var worldPos = _xform.GetWorldPosition(xform);
            if (progress <= 0f || !bounds.Contains(worldPos))
                continue;

            handle.SetTransform(Matrix3x2.Multiply(rotationMatrix, Matrix3Helpers.CreateTranslation(worldPos)));
            handle.DrawRect(comp.BarBounds, comp.BarBackgroundColor);

            var inner = new Box2(comp.BarBounds.BottomLeft + comp.BarBorder, comp.BarBounds.TopRight - comp.BarBorder);
            var fill = inner with { Top = inner.Bottom + inner.Height * progress };
            handle.DrawRect(fill, Color.InterpolateBetween(comp.BarEmptyColor, comp.BarFullColor, progress));
        }

        handle.SetTransform(Matrix3x2.Identity);
    }
}
