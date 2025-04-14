// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// CREATED BY Goldminermac
// https://github.com/space-wizards/space-station-14/pull/31149
// LICENSED UNDER THE MIT LICENSE
// SEE README.MD AND LICENSE.TXT IN THE ROOT OF THIS REPOSITORY FOR MORE INFORMATION

using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Speech;

public sealed class VulgarAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VulgarAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, VulgarAccentComponent component)
    {
        string[] messageWords = message.Split(" ");

        for (int i = 0; i < messageWords.Length; i++)
        {
            //Every word has a percentage chance to be replaced by a random swear word from the component's array.
            if (_random.Prob(component.SwearProb))
            {
                if (!_prototypeManager.TryIndex(component.Pack, out var messagePack))
                    return message;


                string swearWord = _loc.GetString(_random.Pick(messagePack.Values));
                messageWords[i] = swearWord;
            }
        }

        return string.Join(" ", messageWords);
    }

    public void OnAccentGet(EntityUid uid, VulgarAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}