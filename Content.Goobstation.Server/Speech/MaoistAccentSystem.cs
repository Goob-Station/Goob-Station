// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 bedroomvampire <leannetoni@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Goobstation.Common.Speech;
using Content.Server.Speech;

namespace Content.Goobstation.Server.Speech;

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