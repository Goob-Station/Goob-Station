// SPDX-FileCopyrightText: 2022 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2022 hubismal <47284081+hubismal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Content.Server.Chat.Systems;
using Content.Shared.Speech;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Random;

namespace Content.Server.Speech
{
    public sealed class SpeechSoundSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IPrototypeManager _protoManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SpeechComponent, EntitySpokeEvent>(OnEntitySpoke);
        }

        public SoundSpecifier? GetSpeechSound(Entity<SpeechComponent> ent, string message)
        {
            if (ent.Comp.SpeechSounds == null)
                return null;

            // Play speech sound
            SoundSpecifier? contextSound;
            var prototype = _protoManager.Index<SpeechSoundsPrototype>(ent.Comp.SpeechSounds);

            // Different sounds for ask/exclaim based on last character
            contextSound = message[^1] switch
            {
                '?' => prototype.AskSound,
                '!' => prototype.ExclaimSound,
                _ => prototype.SaySound
            };

            // Use exclaim sound if most characters are uppercase.
            int uppercaseCount = 0;
            for (int i = 0; i < message.Length; i++)
            {
                if (char.IsUpper(message[i]))
                    uppercaseCount++;
            }
            if (uppercaseCount > (message.Length / 2))
            {
                contextSound = prototype.ExclaimSound;
            }

            var scale = (float) _random.NextGaussian(1, prototype.Variation);
            contextSound.Params = ent.Comp.AudioParams.WithPitchScale(scale);
            return contextSound;
        }

        private void OnEntitySpoke(EntityUid uid, SpeechComponent component, EntitySpokeEvent args)
        {
            if (component.SpeechSounds == null)
                return;

            var currentTime = _gameTiming.CurTime;
            var cooldown = TimeSpan.FromSeconds(component.SoundCooldownTime);

            // Ensure more than the cooldown time has passed since last speaking
            if (currentTime - component.LastTimeSoundPlayed < cooldown)
                return;

            var sound = GetSpeechSound((uid, component), args.Message);
            component.LastTimeSoundPlayed = currentTime;
            _audio.PlayPvs(sound, uid);
        }
    }
}