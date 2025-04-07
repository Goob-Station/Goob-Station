// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader;

[Serializable, NetSerializable]
public sealed class CartridgeUiMessage : BoundUserInterfaceMessage
{
    public CartridgeMessageEvent MessageEvent;

    public CartridgeUiMessage(CartridgeMessageEvent messageEvent)
    {
        MessageEvent = messageEvent;
    }
}

[Serializable, NetSerializable]
public abstract class CartridgeMessageEvent : EntityEventArgs
{
    [NonSerialized]
    public EntityUid User;
    public NetEntity LoaderUid;

    [NonSerialized]
    public EntityUid Actor;
}