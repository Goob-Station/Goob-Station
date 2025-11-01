// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Zekins <zekins3366@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Corvax.Speech.Synthesis;
using Content.Client._Corvax.Speech.Synthesis.System;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private List<BarkPrototype> _barkPrototypes = new();

    private void InitializeBarkVoice()
    {

        BarkVoiceButton.OnItemSelected += args =>
        {
            BarkVoiceButton.SelectId(args.Id);
            SetBarkVoice(_barkPrototypes[args.Id].ID);
            PlayPreviewBark();
        };

        BarkVoicePlayButton.OnPressed += _ => PlayPreviewBark();
    }

    private void UpdateBarkVoice()
    {
        if (Profile is null)
            return;

        _barkPrototypes = _prototypeManager
            .EnumeratePrototypes<BarkPrototype>()
            .Where(o => o.RoundStart &&
                        (o.SpeciesWhitelist is null ||
                         o.SpeciesWhitelist.Contains(Profile.Species)))
            .OrderBy(o => Loc.GetString(o.ID))
            .ToList();

        BarkVoiceButton.Clear();

        var selectedBarkId = -1;
        for (var i = 0; i < _barkPrototypes.Count; i++)
        {
            var bark = _barkPrototypes[i];
            if (bark.ID == Profile.BarkVoice)
                selectedBarkId = i;

            BarkVoiceButton.AddItem(Loc.GetString(bark.Name), i);
        }

        if (selectedBarkId == -1)
            selectedBarkId = 0;

        BarkVoiceButton.SelectId(selectedBarkId);
        SetBarkVoice(_barkPrototypes[selectedBarkId].ID);
    }

    private void PlayPreviewBark()
    {
        if (Profile is null)
            return;

        _entManager.System<BarkSystem>().RequestPreviewBark(Profile.BarkVoice!);
    }
}
