// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using JetBrains.Annotations;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.UserInterface;

[UsedImplicitly]
public sealed partial class ToggleIntrinsicUIEvent : InstantActionEvent
{
    [DataField("key", customTypeSerializer: typeof(EnumSerializer), required: true)]
    public Enum? Key { get; set; }
}