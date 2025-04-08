// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Notes;

[Serializable, NetSerializable]
public sealed class AdminMessageEuiState(TimeSpan time, AdminMessageEuiState.Message[] messages) : EuiStateBase
{
    public TimeSpan Time { get; } = time;
    public Message[] Messages { get; } = messages;

    [Serializable]
    public sealed class Message(string text, string adminName, DateTime addedOn)
    {
        public string Text = text;
        public string AdminName = adminName;
        public DateTime AddedOn = addedOn;
    }
}

public static class AdminMessageEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class Dismiss(bool permanent) : EuiMessageBase
    {
        public bool Permanent { get; } = permanent;
    }
}