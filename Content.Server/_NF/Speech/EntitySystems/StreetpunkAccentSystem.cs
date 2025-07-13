// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._NF.Speech.Components;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;
using System.Text.RegularExpressions;

namespace Content.Server._NF.Speech.EntitySystems;

public sealed class StreetpunkAccentSystem : EntitySystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    private static readonly Regex RegexIng = new(@"ing\b");
    private static readonly Regex RegexAnd = new(@"\band\b");
    private static readonly Regex RegexDve = new("d've");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StreetpunkAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    // converts left word when typed into the right word. For example typing you becomes ye.
    public string Accentuate(string message, StreetpunkAccentComponent component)
    {
        var msg = message;

        //They shoulda started runnin' an' hidin' from me! <- bit from SouthernDrawl Accent
        msg = RegexIng.Replace(msg, "in'");
        msg = RegexAnd.Replace(msg, "an'");
        msg = RegexDve.Replace(msg, "da");

        msg = _replacement.ApplyReplacements(msg, "streetpunk");


        return msg;
    }

    private void OnAccentGet(EntityUid uid, StreetpunkAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
