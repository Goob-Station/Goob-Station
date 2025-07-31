// SPDX-FileCopyrightText: 2023 dahnte <70238020+dahnte@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Redrover1760 <39284090+Redrover1760@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class FrontalLispSystem : EntitySystem
{
    // @formatter:off
    private static readonly Regex RegexUpperTh = new(@"[T]+[Ss]+|[S]+[Cc]+(?=[IiEeYy]+)|[C]+(?=[IiEeYy]+)|[P][Ss]+|([S]+[Tt]+|[T]+)(?=[Ii]+[Oo]+[Uu]*[Nn]*)|[C]+[Hh]+(?=[Ii]*[Ee]*)|[Z]+|[S]+|[X]+(?=[Ee]+)");
    private static readonly Regex RegexLowerTh = new(@"[t]+[s]+|[s]+[c]+(?=[iey]+)|[c]+(?=[iey]+)|[p][s]+|([s]+[t]+|[t]+)(?=[i]+[o]+[u]*[n]*)|[c]+[h]+(?=[i]*[e]*)|[z]+|[s]+|[x]+(?=[e]+)");
    private static readonly Regex RegexUpperEcks = new(@"[E]+[Xx]+[Cc]*|[X]+");
    private static readonly Regex RegexLowerEcks = new(@"[e]+[x]+[c]*|[x]+");
    // @formatter:on

    [Dependency] private readonly IRobustRandom _random = default!; // Corvax-Localization

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrontalLispComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, FrontalLispComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // handles ts, sc(i|e|y), c(i|e|y), ps, st(io(u|n)), ch(i|e), z, s
        message = RegexUpperTh.Replace(message, "Th"); // Goob Edit
        message = RegexLowerTh.Replace(message, "th");
        // handles ex(c), x
        message = RegexUpperEcks.Replace(message, "Ekth"); // Goob Edit
        message = RegexLowerEcks.Replace(message, "ekth");

        // Corvax-Localization Start
        // с - ш
        message = Regex.Replace(message, @"с", _random.Prob(0.90f) ? "ш" : "с");
        message = Regex.Replace(message, @"С", _random.Prob(0.90f) ? "Ш" : "С");
        // ч - ш
        message = Regex.Replace(message, @"ч", _random.Prob(0.90f) ? "ш" : "ч");
        message = Regex.Replace(message, @"Ч", _random.Prob(0.90f) ? "Ш" : "Ч");
        // ц - ч
        message = Regex.Replace(message, @"ц", _random.Prob(0.90f) ? "ч" : "ц");
        message = Regex.Replace(message, @"Ц", _random.Prob(0.90f) ? "Ч" : "Ц");
        // т - ч
        message = Regex.Replace(message, @"\B[т](?![АЕЁИОУЫЭЮЯаеёиоуыэюя])", _random.Prob(0.90f) ? "ч" : "т");
        message = Regex.Replace(message, @"\B[Т](?![АЕЁИОУЫЭЮЯаеёиоуыэюя])", _random.Prob(0.90f) ? "Ч" : "Т");
        // з - ж
        message = Regex.Replace(message, @"з", _random.Prob(0.90f) ? "ж" : "з");
        message = Regex.Replace(message, @"З", _random.Prob(0.90f) ? "Ж" : "З");
        // Corvax-Localization End

        args.Message = message;
    }
}
