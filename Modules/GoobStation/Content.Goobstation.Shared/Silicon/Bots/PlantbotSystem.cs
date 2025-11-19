// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Emag.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Silicon.Bots;

/// <summary>
/// Handles emagging Plantbots
/// </summary>
public sealed class PlantbotSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlantbotComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, PlantbotComponent comp, ref GotEmaggedEvent args)
    {
        _audio.PlayPredicted(comp.EmagSparkSound, uid, args.UserUid);

        comp.IsEmagged = true;
        args.Handled = true;
    }
}
