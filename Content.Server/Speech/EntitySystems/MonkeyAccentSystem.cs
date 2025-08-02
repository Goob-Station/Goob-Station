// SPDX-FileCopyrightText: 2021 Pancake <Pangogie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class MonkeyAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MonkeyAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        var words = message.Split();
        var accentedMessage = new StringBuilder(message.Length + 2);

        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];

            if (_random.NextDouble() >= 0.5)
            {
                if (word.Length > 1)
                {
                    foreach (var _ in word)
                    {
                        accentedMessage.Append('У');  // CorvaxGoob-Localization
                    }

                    if (_random.NextDouble() >= 0.3)
                        accentedMessage.Append('К');  // CorvaxGoob-Localization
                }
                else
                    accentedMessage.Append('У');  // CorvaxGoob-Localization
            }
            else
            {
                foreach (var _ in word)
                {
                    if (_random.NextDouble() >= 0.8)
                        accentedMessage.Append('Г');  // CorvaxGoob-Localization
                    else
                        accentedMessage.Append('А');  // CorvaxGoob-Localization
                }

            }

            if (i < words.Length - 1)
                accentedMessage.Append(' ');
        }

        accentedMessage.Append('!');

        return accentedMessage.ToString();
    }

    private void OnAccent(EntityUid uid, MonkeyAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
