using System.Numerics;
using Content.Client.Cooldown;
using Content.Shared.Timing;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client.Hands;

public sealed partial class ShowHandItemOverlay
{
    [Dependency] private IUserInterfaceManager _ui = default!;

    private UseDelaySystem? _delay;
    private readonly CooldownGraphic _cooldownGraphic = CreateCooldownGraphic();

    private static CooldownGraphic CreateCooldownGraphic()
    {
        var graphic = new CooldownGraphic
        {
            SetSize = new Vector2(64, 64),
        };

        graphic.Arrange(UIBox2.FromDimensions(Vector2.Zero, new Vector2(64, 64)));
        return graphic;
    }

    private void DrawCooldown(in OverlayDrawArgs args, EntityUid handEntity, Vector2 mousePos, Control? vpControl)
    {
        if (!_entMan.TryGetComponent<UseDelayComponent>(handEntity, out var delay))
            return;

        _delay ??= _entMan.System<UseDelaySystem>();
        var cooldown = _delay.GetLastEndingDelay((handEntity, delay));
        if (cooldown.StartTime == cooldown.EndTime || cooldown.Length == TimeSpan.Zero)
            return;

        _cooldownGraphic.FromTime(cooldown.StartTime, cooldown.EndTime);

        vpControl?.AddChild(_cooldownGraphic);
        _ui.RenderControl(args.RenderHandle, _cooldownGraphic, (Vector2i) (mousePos - _cooldownGraphic.PixelSize / 2));
        vpControl?.RemoveChild(_cooldownGraphic);
    }
}
