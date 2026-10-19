// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.MalfAI;

// Server -> Client: open the Malf voice modulator window.
[Serializable, NetSerializable]
public sealed class MalfVoiceModulatorOpenUiEvent : EntityEventArgs
{
}

// Client -> Server: submit the chosen AI name.
[Serializable, NetSerializable]
public sealed class MalfVoiceModulatorSubmitNameEvent : EntityEventArgs
{
    public readonly string Name;

    public MalfVoiceModulatorSubmitNameEvent(string name)
    {
        Name = name;
    }
}
