// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random; // Pirate edit

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    // Pirate edit start
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex RegexLowerS = new("s+|с+|ш+|щ+|ц+|ч+");
    private static readonly Regex RegexUpperS = new("S+|С+|Ш+|Щ+|Ц+|Ч+");
    // Pirate edit end
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // hissss
        message = RegexLowerS.Replace(message, m => new string(m.Value[0], _random.Next(2, 4))); // Pirate edit
        // hiSSS
        message = RegexUpperS.Replace(message, m => new string(m.Value[0], _random.Next(2, 4))); // Pirate edit
        // ekssit
        message = RegexInternalX.Replace(message, "$1kss");
        // ecks
        message = RegexLowerEndX.Replace(message, "ecks$1");
        // eckS
        message = RegexUpperEndX.Replace(message, "ECKS$1");

        args.Message = message;
    }
}