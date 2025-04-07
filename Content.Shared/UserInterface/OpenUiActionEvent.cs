// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
//
// SPDX-License-Identifier: MIT
using Content.Shared.Actions;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.UserInterface;

public sealed partial class OpenUiActionEvent : InstantActionEvent
{
    [DataField(required: true, customTypeSerializer: typeof(EnumSerializer))]
    public Enum? Key { get; private set; }
}