using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class MaoistAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexCK = new("[cCkK]");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MaoistAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MaoistAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // SShit$$eKKK
        message = RegexLowerS.Replace(message, "$$$$");
        message = RegexUpperS.Replace(message, "SS");
        // KKKommand
        message = RegexCK.Replace(message, "KKK");

        args.Message = message;
    }
}
