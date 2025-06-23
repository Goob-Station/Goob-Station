// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ItemToggle;

[RegisterComponent]
public sealed partial class ItemToggleVisualsComponent : Component
{
    [DataField]
    public string? HeldPrefixOn = "on";

    [DataField]
    public string? HeldPrefixOff = "off";
}

[Serializable, NetSerializable]
public enum ItemToggleVisuals
{
    State,
    Layer,
}
