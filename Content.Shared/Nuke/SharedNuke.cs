// SPDX-FileCopyrightText: 2021 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Nuke
{
    public enum NukeVisualLayers
    {
        Base,
        Unlit
    }

    [NetSerializable, Serializable]
    public enum NukeVisuals
    {
        Deployed,
        State,
    }

    [NetSerializable, Serializable]
    public enum NukeVisualState
    {
        Idle,
        Armed,
        YoureFucked
    }

    [Serializable, NetSerializable]
    public enum NukeUiKey : byte
    {
        Key
    }

    public enum NukeStatus : byte
    {
        AWAIT_DISK,
        AWAIT_CODE,
        AWAIT_ARM,
        ARMED,
        COOLDOWN
    }

    [Serializable, NetSerializable]
    public sealed class NukeUiState : BoundUserInterfaceState
    {
        public bool DiskInserted;
        public NukeStatus Status;
        public int RemainingTime;
        public int CooldownTime;
        public bool IsAnchored;
        public int EnteredCodeLength;
        public int MaxCodeLength;
        public bool AllowArm;
    }

    [Serializable, NetSerializable]
    public sealed partial class NukeDisarmDoAfterEvent : SimpleDoAfterEvent
    {
    }
}