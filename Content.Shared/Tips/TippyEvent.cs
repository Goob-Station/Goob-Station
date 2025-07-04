// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Tips;

[Serializable, NetSerializable]
public sealed class TippyEvent : EntityEventArgs
{
    public TippyEvent(string msg)
    {
        Msg = msg;
    }

    public string Msg;
    public string? Proto;
    public float SpeakTime = 5;
    public float SlideTime = 3;
    public float WaddleInterval = 0.5f;
}