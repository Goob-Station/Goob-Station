// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Nuke
{
    public abstract partial class SharedNukeComponent : Component
    {
        public const string NukeDiskSlotId = "Nuke";
    }

    [Serializable, NetSerializable]
    public sealed class NukeAnchorMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class NukeKeypadMessage : BoundUserInterfaceMessage
    {
        public int Value;

        public NukeKeypadMessage(int value)
        {
            Value = value;
        }
    }

    [Serializable, NetSerializable]
    public sealed class NukeKeypadClearMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class NukeKeypadEnterMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class NukeArmedMessage : BoundUserInterfaceMessage
    {
    }
}