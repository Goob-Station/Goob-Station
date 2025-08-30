// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Speech;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.TypingIndicator;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects; // EntitySessionEventArgs
using Robust.Shared.Random;
using System.Collections.Generic;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Speech;

/// <summary>
/// Plays typing start and message send sounds when a wearer of the security gas mask types/sends chat.
/// </summary>
public sealed class SecurityGasMaskTypingSoundSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    // Audio resources (relative to Resources/Audio -> "/Audio" in content paths)
    private readonly string[] TypingOnSounds = new[]
    {
        "/Audio/_Goobstation/Items/other/SecurityGasMask/on1.wav",
        "/Audio/_Goobstation/Items/other/SecurityGasMask/on2.wav",
    };
    private readonly string[] TypingOffSounds = new[]
    {
        "/Audio/_Goobstation/Items/other/SecurityGasMask/off1.wav",
        "/Audio/_Goobstation/Items/other/SecurityGasMask/off2.wav",
        "/Audio/_Goobstation/Items/other/SecurityGasMask/off3.wav",
        "/Audio/_Goobstation/Items/other/SecurityGasMask/off4.wav",
    };

    // 50% quieter (approx. -6 dB) and reduced audible range
    private static readonly AudioParams MaskAudioParams = AudioParams.Default
        .WithVolume(-6f)
        .WithMaxDistance(7f);

    private readonly HashSet<EntityUid> _currentlyTyping = new();

    public override void Initialize()
    {
        base.Initialize();

        // When client starts/stops typing in chat input.
        SubscribeAllEvent<TypingChangedEvent>(OnTypingChanged);
        // When an entity speaks/whispers (after chat pipeline).
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);
    }

    private void PlayMaskSound(EntityUid uid, string[] options)
    {
        if (!HasComp<SecurityGasMaskAccentComponent>(uid))
            return;

        var path = _random.Pick(options);
        _audio.PlayPvs(path, uid, MaskAudioParams);
    }

    private void OnTypingChanged(TypingChangedEvent ev, EntitySessionEventArgs args)
    {
        // Resolve the typing player's entity.
        var uid = args.SenderSession.AttachedEntity;
        if (uid is not { Valid: true })
            return;

        switch (ev.State)
        {
            case TypingIndicatorState.Typing:
                if (!_currentlyTyping.Contains(uid.Value))
                {
                    PlayMaskSound(uid.Value, TypingOnSounds);
                    _currentlyTyping.Add(uid.Value);
                }
                break;
            case TypingIndicatorState.Idle:
            case TypingIndicatorState.None:
                _currentlyTyping.Remove(uid.Value);
                break;
        }
    }

    private void OnEntitySpoke(EntitySpokeEvent ev)
    {
        PlayMaskSound(ev.Source, TypingOffSounds);
    }
}
