// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class StutteringSystem : SharedStutteringSystem
    {
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        // Regex of characters to stutter.
        private static readonly Regex Stutter = new(@"[b-df-hj-np-tv-wxyz]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override void Initialize()
        {
            SubscribeLocalEvent<StutteringAccentComponent, AccentGetEvent>(OnAccent);
        }

        public override void DoStutter(EntityUid uid, TimeSpan time, bool refresh, StatusEffectsComponent? status = null)
        {
            if (!Resolve(uid, ref status, false))
                return;

            _statusEffectsSystem.TryAddStatusEffect<StutteringAccentComponent>(uid, StutterKey, time, refresh, status);
        }

        private void OnAccent(EntityUid uid, StutteringAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message, component);
        }

        public string Accentuate(string message, StutteringAccentComponent component)
        {
            var length = message.Length;

            var finalMessage = new StringBuilder();

            string newLetter;

            for (var i = 0; i < length; i++)
            {
                newLetter = message[i].ToString();
                if (Stutter.IsMatch(newLetter) && _random.Prob(component.MatchRandomProb))
                {
                    if (_random.Prob(component.FourRandomProb))
                    {
                        newLetter = $"{newLetter}-{newLetter}-{newLetter}-{newLetter}";
                    }
                    else if (_random.Prob(component.ThreeRandomProb))
                    {
                        newLetter = $"{newLetter}-{newLetter}-{newLetter}";
                    }
                    else if (_random.Prob(component.CutRandomProb))
                    {
                        newLetter = "";
                    }
                    else
                    {
                        newLetter = $"{newLetter}-{newLetter}";
                    }
                }

                finalMessage.Append(newLetter);
            }

            return finalMessage.ToString();
        }
    }
}