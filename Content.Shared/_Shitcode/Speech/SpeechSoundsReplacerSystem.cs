// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory.Events;
using Content.Shared.Speech;

namespace Content.Shared._Goobstation.Speech;

/// <summary>
/// System that replace your speech sound when you wearing specific clothing
/// </summary>
public sealed class SpeechSoundsReplacerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeechSoundsReplacerComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<SpeechSoundsReplacerComponent, GotUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(Entity<SpeechSoundsReplacerComponent> replacer, ref GotEquippedEvent args)
    {
        if (EntityManager.TryGetComponent<SpeechComponent>(args.Equipee, out var speech))
        {
            replacer.Comp.PreviousSound = speech.SpeechSounds;
            speech.SpeechSounds = replacer.Comp.SpeechSounds;
        }
    }

    private void OnUnequip(Entity<SpeechSoundsReplacerComponent> replacer, ref GotUnequippedEvent args)
    {
        if (EntityManager.TryGetComponent<SpeechComponent>(args.Equipee, out var speech))
        {
            speech.SpeechSounds = replacer.Comp.PreviousSound;
            replacer.Comp.PreviousSound = null;
        }
    }
}