// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 hubismal <47284081+hubismal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Speech
{
    [Prototype("speechSounds")]
    public sealed partial class SpeechSoundsPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        //Variation is here instead of in SharedSpeechComponent since some sets of
        //sounds may require more fine tuned pitch variation than others.
        [DataField("variation")]
        public float Variation { get; set; } = 0.1f;

        [DataField("saySound")]
        public SoundSpecifier SaySound { get; set; } = new SoundPathSpecifier("/Audio/Voice/Talk/speak_2.ogg");

        [DataField("askSound")]
        public SoundSpecifier AskSound { get; set; } = new SoundPathSpecifier("/Audio/Voice/Talk/speak_2_ask.ogg");

        [DataField("exclaimSound")]
        public SoundSpecifier ExclaimSound { get; set; } = new SoundPathSpecifier("/Audio/Voice/Talk/speak_2_exclaim.ogg");
    }
}