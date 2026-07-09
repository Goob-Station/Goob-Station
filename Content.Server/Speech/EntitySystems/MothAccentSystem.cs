// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random; // Pirate edit

namespace Content.Server.Speech.EntitySystems;

public sealed class MothAccentSystem : EntitySystem
{
    // Pirate edit start
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Regex RegexLowerBuzz = new("z+|з+|ж+");
    private static readonly Regex RegexUpperBuzz = new("Z+|З+|Ж+");
    // Pirate edit end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // buzzz
        // // Pirate edit start
        message = RegexLowerBuzz.Replace(message, m =>
        {
            var first = m.Value[0];
            var count = _random.Next(2, 4);
            if (first == 'ж') return "з" + new string('ж', count - 1);
            return new string(first, count);
        });
        // buZZZ
        message = RegexUpperBuzz.Replace(message, m =>
        {
            var first = m.Value[0];
            var count = _random.Next(2, 4);
            if (first == 'Ж') return "З" + new string('Ж', count - 1);
            return new string(first, count);
        });
        // Pirate edit end

        args.Message = message;
    }
}