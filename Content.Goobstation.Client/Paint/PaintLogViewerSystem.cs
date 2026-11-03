using System.Numerics;
using Content.Goobstation.Shared.Paint;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Goobstation.Client.Paint;

public sealed partial class PaintLogViewerSystem : EntitySystem
{
    [Dependency] private IClyde _clyde = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PaintLogImageResponseEvent>(OnImageResponse);
    }

    private void OnImageResponse(PaintLogImageResponseEvent ev)
    {
        var window = new DefaultWindow
        {
            Title = Loc.GetString("paint-log-viewer-title", ("token", ev.Token)),
            MinSize = new Vector2(420, 440),
        };

        if (ev.Image is not { } image)
        {
            window.Contents.AddChild(new Label { Text = Loc.GetString("paint-log-viewer-missing") });
            window.OpenCentered();
            return;
        }

        var texture = PaintTextures.Create(_clyde, image, $"paint-log-{ev.Token}");
        window.Contents.AddChild(new PaintCanvasControl
        {
            Interactive = false,
            Texture = texture,
            ImageSize = image.Size,
            HorizontalExpand = true,
            VerticalExpand = true,
        });
        window.OnClose += texture.Dispose;
        window.OpenCentered();
    }
}
