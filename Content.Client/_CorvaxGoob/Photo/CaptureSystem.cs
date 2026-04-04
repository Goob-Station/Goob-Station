using Content.Shared._CorvaxGoob.Photo;
using Robust.Client.Graphics;
using SixLabors.ImageSharp;
using System.IO;

namespace Content.Client._CorvaxGoob.Photo;

public sealed class CaptureSystem : EntitySystem
{
    [Dependency] private readonly IClyde _clyde = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CaptureScreenRequestEvent>(RequestCaptureScreen);
    }

    private async void RequestCaptureScreen(CaptureScreenRequestEvent ev)
    {
        var capture = await _clyde.ScreenshotAsync(ScreenshotType.Final);
        using (MemoryStream ms = new MemoryStream())
        {
            await capture.SaveAsPngAsync(ms);
            RaiseNetworkEvent(new CaptureScreenResponseEvent(ms.ToArray()));
        }
    }
}
