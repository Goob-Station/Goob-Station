// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chat;
using Content.Shared.Inventory;

namespace Content.Shared.Radio;

public sealed class GetDefaultRadioChannelEvent : EntityEventArgs, IInventoryRelayEvent
{
    /// <summary>
    ///     Id of the default <see cref="RadioChannelPrototype"/> that will get addressed when using the
    ///     department/default channel prefix. See <see cref="SharedChatSystem.DefaultChannelKey"/>.
    /// </summary>
    public string? Channel;

    public SlotFlags TargetSlots => ~SlotFlags.POCKET;
}