using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.News.Tags;

public sealed partial class NewsImage : IDocumentTag
{
    [Dependency] private IResourceCache _cache = default!;

    public NewsImage()
    {
        IoCManager.InjectDependencies(this);
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!args.TryGetValue("Path", out var path))
        {
            Logger.GetSawmill("news").Error("NewsImage needs a Path");
            return false;
        }

        if (GetTexture(path, args.GetValueOrDefault("State")) is not { } texture)
        {
            Logger.GetSawmill("news").Error($"NewsImage couldn't load {path}");
            return false;
        }

        var scale = args.TryGetValue("Scale", out var scaleText) ? NewsTagArgs.Float(scaleText) : 1f;
        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalAlignment = NewsTagArgs.HAlign(args, Control.HAlignment.Center),
            VerticalAlignment = Control.VAlignment.Bottom,
            Margin = NewsTagArgs.Margin(args),
            SeparationOverride = 2,
        };

        box.AddChild(new TextureRect
        {
            Texture = texture,
            TextureScale = new Vector2(scale, scale),
            Stretch = TextureRect.StretchMode.KeepCentered,
            HorizontalAlignment = Control.HAlignment.Center,
        });

        if (args.TryGetValue("Caption", out var caption))
        {
            box.AddChild(new Label
            {
                Text = Loc.GetString(caption),
                HorizontalAlignment = Control.HAlignment.Center,
                StyleClasses = { HullStyleClasses.Muted },
            });
        }

        control = box;
        return true;
    }

    private Texture? GetTexture(string path, string? state)
    {
        if (state == null)
            return _cache.TryGetResource<TextureResource>(new ResPath(path), out var texture) ? texture.Texture : null;

        return _cache.TryGetResource<RSIResource>(new ResPath(path), out var rsi) && rsi.RSI.TryGetState(state, out var rsiState)
            ? rsiState.Frame0
            : null;
    }
}
