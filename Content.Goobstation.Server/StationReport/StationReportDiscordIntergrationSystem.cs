using System;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.StationReport;
using Robust.Shared.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;

namespace Content.Goobstation.Server.StationReportDiscordIntergrationSystem;

public sealed class StationReportDiscordIntergrationSystem : EntitySystem
{
    //thank you Timfa for writing this code
    private static readonly HttpClient client = new();
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private string? _webhookUrl;

    public override void Initialize()
    {
        base.Initialize();

        //subscribes to the endroundevent and Stationreportevent
        SubscribeLocalEvent<StationReportEvent>(OnStationReportReceived);

        // Keep track of CCVar value, update if changed
        _cfg.OnValueChanged(GoobCVars.StationReportDiscordWebHook, url => _webhookUrl = url, true);
    }

    public static string? report;

    private static readonly TagReplacement[] _replacements =
    {
        new(@"\[/?bold\]", @"**"),
        new(@"\[/?italic\]", @"_"),
        new(@"\[/?mono\]", @"__"),
        new(@">", @""),
        new(@"\[h1\]", @"# "),
        new(@"\[h2\]", @"## "),
        new(@"\[h3\]", @"### "),
        new(@"\[h4\]", @"-# "),
        new(@"\[/h[0-9]\]", @""),
        new(@"\[head=1\]", @"# "),
        new(@"\[head=2\]", @"## "),
        new(@"\[head=3\]", @"### "),
        new(@"\[head=4\]", @"-# "),
        new(@"\[/head\]", @""),
        new(@"\[/?color(=[#0-9a-zA-Z]+)?\]", @"")
    };

    private void OnStationReportReceived(StationReportEvent ev)
    {
        report = ev.StationReportText;

        if (string.IsNullOrWhiteSpace(report))
            return;

        foreach (var replacement in _replacements)
            report = Regex.Replace(report, replacement.Tag, replacement.Replacement);

        // Run async without blocking
        _ = SendMessageAsync(report);
    }

    private async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(_webhookUrl))
            return;

        var payload = new { content = message };
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(_webhookUrl, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {

        }
    }

    public struct TagReplacement
    {
        public string Tag, Replacement;
        public TagReplacement(string tag, string replacement)
        {
            Tag = tag;
            Replacement = replacement;
        }
    }
}
