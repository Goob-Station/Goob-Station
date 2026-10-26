using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Paint;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Network;
using SixLabors.ImageSharp;

namespace Content.Goobstation.Server.Paint;

[AdminCommand(AdminFlags.Fun)]
public sealed partial class PaintLoadUrlCommand : LocalizedEntityCommands
{
    private const int MaxDownloadBytes = 8 * 1024 * 1024;
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromSeconds(15);

    [Dependency] private IHttpClientHolder _http = default!;
    [Dependency] private PaintSurfaceSystem _paint = default!;
    [Dependency] private PaintLogSystem _paintLog = default!;

    public override string Command => "paintloadurl";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var netEntity)
            || !EntityManager.TryGetEntity(netEntity, out var target)
            || !EntityManager.HasComponent<PaintSurfaceComponent>(target))
        {
            shell.WriteError(Loc.GetString("cmd-paintloadurl-bad-target", ("target", args[0])));
            return;
        }

        if (!Uri.TryCreate(args[1], UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            shell.WriteError(Loc.GetString("cmd-paintloadurl-bad-url"));
            return;
        }

        shell.WriteLine(Loc.GetString("cmd-paintloadurl-downloading", ("url", uri.ToString())));

        var surface = EntityManager.GetComponent<PaintSurfaceComponent>(target.Value);
        var size = surface.Size;
        var background = surface.Background;

        PaintImage image;
        try
        {
            var data = await Download(uri);
            image = await Task.Run(() => PaintImageImporter.Import(data, size, background));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or InvalidDataException)
        {
            shell.WriteError(Loc.GetString("cmd-paintloadurl-download-failed", ("error", e.Message)));
            return;
        }
        catch (Exception e) when (e is ImageFormatException or NotSupportedException)
        {
            shell.WriteError(Loc.GetString("cmd-paintloadurl-bad-image", ("error", e.Message)));
            return;
        }

        if (!EntityManager.TryGetComponent<PaintSurfaceComponent>(target, out var current)
            || !_paint.SetImage((target.Value, current), image))
        {
            shell.WriteError(Loc.GetString("cmd-paintloadurl-bad-target", ("target", args[0])));
            return;
        }

        _paintLog.LogImageLoaded(target.Value, shell.Player, uri.ToString(), image);
        shell.WriteLine(Loc.GetString("cmd-paintloadurl-success",
            ("target", EntityManager.ToPrettyString(target.Value))));
    }

    private async Task<byte[]> Download(Uri uri)
    {
        using var cancel = new CancellationTokenSource(DownloadTimeout);
        using var response = await _http.Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancel.Token);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength > MaxDownloadBytes)
            throw new InvalidDataException(Loc.GetString("cmd-paintloadurl-too-large"));

        await using var stream = await response.Content.ReadAsStreamAsync(cancel.Token);
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(buffer, cancel.Token)) > 0)
        {
            if (memory.Length + read > MaxDownloadBytes)
                throw new InvalidDataException(Loc.GetString("cmd-paintloadurl-too-large"));

            memory.Write(buffer, 0, read);
        }

        return memory.ToArray();
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHint(Loc.GetString("cmd-paintloadurl-hint-target")),
            2 => CompletionResult.FromHint(Loc.GetString("cmd-paintloadurl-hint-url")),
            _ => CompletionResult.Empty,
        };
    }
}
