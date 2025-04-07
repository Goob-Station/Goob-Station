// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Bixkitts <72874643+Bixkitts@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Weapons.Melee;

/// <summary>
/// This handles <see cref="MeleeSoundComponent"/>
/// </summary>
public sealed class MeleeSoundSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public const float DamagePitchVariation = 0.05f;

    /// <summary>
    /// Plays the SwingSound from a weapon component
    /// for immediate feedback, misses and such
    /// (Swinging a weapon goes "whoosh" whether it hits or not)
    /// </summary>
    public void PlaySwingSound(EntityUid userUid, EntityUid weaponUid, MeleeWeaponComponent weaponComponent)
    {
        _audio.PlayPredicted(weaponComponent.SwingSound, weaponUid, userUid);
    }

    /// <summary>
    /// Takes a "damageType" string as an argument and uses it to
    /// search one of the various Dictionaries in the MeleeSoundComponent
    /// for a sound to play, and falls back if that fails
    /// </summary>
    /// <param name="damageType"> Serves as a lookup key for a hit sound </param>
    /// <param name="hitSoundOverride"> A sound can be supplied by the <see cref="MeleeHitEvent"/> itself to override everything else </param>
    public void PlayHitSound(EntityUid targetUid, EntityUid? userUid, string? damageType, SoundSpecifier? hitSoundOverride, MeleeWeaponComponent weaponComponent)
    {
        var hitSound      = weaponComponent.HitSound;
        var noDamageSound = weaponComponent.NoDamageSound;

        var playedSound = false;

        if (Deleted(targetUid))
            return;

        // hitting can obv destroy an entity so we play at coords and not following them
        var coords = Transform(targetUid).Coordinates;
        // Play sound based off of highest damage type.
        if (TryComp<MeleeSoundComponent>(targetUid, out var damageSoundComp))
        {
            if (damageType == null && damageSoundComp.NoDamageSound != null)
            {
                _audio.PlayPredicted(damageSoundComp.NoDamageSound, coords, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
            else if (damageType != null && damageSoundComp.SoundTypes?.TryGetValue(damageType, out var damageSoundType) == true)
            {
                _audio.PlayPredicted(damageSoundType, coords, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
            else if (damageType != null && damageSoundComp.SoundGroups?.TryGetValue(damageType, out var damageSoundGroup) == true)
            {
                _audio.PlayPredicted(damageSoundGroup, coords, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
        }

        // Use weapon sounds if the thing being hit doesn't specify its own sounds.
        if (!playedSound)
        {
            if (hitSoundOverride != null)
            {
                _audio.PlayPredicted(hitSoundOverride, coords, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
            else if (hitSound != null)
            {
                _audio.PlayPredicted(hitSound, coords, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
            else
            {
                _audio.PlayPredicted(noDamageSound, coords, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
        }

        // Fallback to generic sounds.
        if (!playedSound)
        {
            switch (damageType)
            {
                // Unfortunately heat returns caustic group so can't just use the damagegroup in that instance.
                case "Burn":
                case "Heat":
                case "Radiation":
                case "Cold":
                    _audio.PlayPredicted(new SoundPathSpecifier("/Audio/Items/welder.ogg"), targetUid, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                    break;
                // No damage, fallback to tappies
                case null:
                    _audio.PlayPredicted(new SoundCollectionSpecifier("WeakHit"), targetUid, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                    break;
                case "Brute":
                    _audio.PlayPredicted(new SoundCollectionSpecifier("MetalThud"), targetUid, userUid, AudioParams.Default.WithVariation(DamagePitchVariation));
                    break;
            }
        }
    }

}