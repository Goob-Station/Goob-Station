// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Eugene <gish.ee18@gmail.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2024 TakoDragon <69509841+BackeTako@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Text;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class SpanishAccentSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<SpanishAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string Accentuate(string message)
        {
            // Insert E before every S
            message = InsertS(message);
            // If a sentence ends with ?, insert a reverse ? at the beginning of the sentence
            message = ReplacePunctuation(message);
            return message;
        }

        private string InsertS(string message)
        {
            // Replace every new Word that starts with s/S
            var msg = message.Replace(" s", " es").Replace(" S", " Es");

            // Still need to check if the beginning of the message starts
            if (msg.StartsWith("s", StringComparison.Ordinal))
            {
                return msg.Remove(0, 1).Insert(0, "es");
            }
            else if (msg.StartsWith("S", StringComparison.Ordinal))
            {
                return msg.Remove(0, 1).Insert(0, "Es");
            }

            return msg;
        }

        private string ReplacePunctuation(string message)
        {
            var sentences = AccentSystem.SentenceRegex.Split(message);
            var msg = new StringBuilder();
            foreach (var s in sentences)
            {
                var toInsert = new StringBuilder();
                for (var i = s.Length - 1; i >= 0 && "?!‽".Contains(s[i]); i--)
                {
                    toInsert.Append(s[i] switch
                    {
                        '?' => '¿',
                        '!' => '¡',
                        '‽' => '⸘',
                        _ => ' '
                    });
                }
                if (toInsert.Length == 0)
                {
                    msg.Append(s);
                } else
                {
                    msg.Append(s.Insert(s.Length - s.TrimStart().Length, toInsert.ToString()));
                }
            }
            return msg.ToString();
        }

        private void OnAccent(EntityUid uid, SpanishAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}