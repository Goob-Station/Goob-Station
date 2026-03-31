using Content.Shared._CorvaxGoob.Photo;
using Robust.Client.Graphics;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading.Tasks;
using Content.Client.Viewport;
using Robust.Client.State;

namespace Content.Client._CorvaxGoob.Photo;

public sealed class CaptureSystem : EntitySystem
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IStateManager _state = default!; // eventually, clyde doesnt work, if client uses other renderer?
    //Viewport screenshot captures game scene only, you cant see UI with it

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CaptureScreenRequestEvent>(RequestCaptureScreen);
    }

    private async void RequestCaptureScreen(CaptureScreenRequestEvent ev)
    {
        try
        {
            switch (ev.Type)
            {
                case PhotoCaptureType.Clyde:
                    await CaptureClyde();
                    break;

                case PhotoCaptureType.Viewport:
                    CaptureViewport();
                    break;
            }
        }
        catch
        {
        }
    }

    //They should be async so we never stop main thread

    // CaptureClyde() uses default ss14 renderer (clyde) for screenshots
    private async Task CaptureClyde()
    {
        var capture = await _clyde.ScreenshotAsync(ScreenshotType.Final);

        await using var ms = new MemoryStream();
        await capture.SaveAsJpegAsync(ms); // JPEG IS SAME SHIT FOR YOUR PURPOSES, USE IT INSTEAD OF PNG!!!

        RaiseNetworkEvent(new CaptureScreenResponseEvent(ms.ToArray()));
    }

    // CaptureViewport() uses game scene renderer for screenshots, it may be useful if someone is using linux and for compat uses OpenGL ES 2 instead of clyde
    private void CaptureViewport()
    {
        if (_state.CurrentState is not IMainViewportState state)
            return;
        state.Viewport.Viewport.Screenshot(async screenshot =>
        {
            await using var ms = new MemoryStream();
            await screenshot.SaveAsJpegAsync(ms); // JPEG IS SAME SHIT FOR YOUR PURPOSES, USE IT INSTEAD OF PNG!!!

            RaiseNetworkEvent(new CaptureScreenResponseEvent(ms.ToArray()));
        });
    }
}
