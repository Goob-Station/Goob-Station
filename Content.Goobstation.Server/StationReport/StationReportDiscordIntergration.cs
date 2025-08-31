using System;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Content.Goobstation.Shared.StationReport;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.StationReportDiscordIntergration;

public sealed class StationReportDiscordIntergration : EntitySystem
{
    //thank you Timfa for writing this code
    private static readonly HttpClient client = new HttpClient();
    private const string WebhookUrl = "https://discord.com/chudcity"; // Should be a CCVAR so we can send specific server reports to specific channels/webhooks
    public override void Initialize()
    {
        //subscribes to the endroundevent and Stationreportevent
        SubscribeLocalEvent<StationReportEvent>(OnStationReportReceived);
    }
    public static string? report;
    private static TagReplacement[] _replacements =
        {
            new(@"\[/?bold\]", @"**"), // replace [bold] and [/bold] with **
			new(@"\[/?italic\]", @"_"), // replace [italics] and [/italics] with _
			new(@"\[/?mono\]", @"__"), // replace [mono] with __
            new(@">", @""), // strips > as it causes some issues with discord formatting
			new(@"\[h1\]", @"# "), // replace [h1] with # and then a space (only works at the start of a line in Discord)
			new(@"\[h2\]", @"## "), // replace [h2] with ## and then a space (only works at the start of a line in Discord)
			new(@"\[h3\]", @"### "), // replace [h3] with ### and then a space (only works at the start of a line in Discord)
			new(@"\[h4\]", @"-# "), // replace [h4] with -# and then a space (only works at the start of a line in Discord)
			new(@"\[/h[0-9]\]", @""), // strips [/h1], [/h2], [/h3], [/h4], etc.
		
			new(@"\[head=1\]", @"# "), // replace [head=1] with # and then a space (only works at the start of a line in Discord)
			new(@"\[head=2\]", @"## "), // replace [head=2] with ## and then a space (only works at the start of a line in Discord)
			new(@"\[head=3\]", @"### "), // replace [head=3] with ### and then a space (only works at the start of a line in Discord)
			new(@"\[head=4\]", @"-# "), // replace [head=4] with -# and then a space (only works at the start of a line in Discord)
			new(@"\[/head\]", @""), // strips [/head], etc. so no head?
		
			new(@"\[/?color(=[#0-9a-zA-Z]+)?\]", @"") // strips [color] tags. (Discord does not have a suitable replacement element)
		};
    private void OnStationReportReceived(StationReportEvent ev)
    {
        report = ev.StationReportText;
        // If report is null, bail early
        if (string.IsNullOrWhiteSpace(report))
        {
            return;
        }

        foreach (TagReplacement replacement in _replacements)
        {
            report = Regex.Replace(report, replacement.Tag, replacement.Replacement);
        }
        // Run async without blocking
        _ = SendMessageAsync(report);
    }
    public static async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var payload = new
        {
            content = message
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(WebhookUrl, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            return;
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
