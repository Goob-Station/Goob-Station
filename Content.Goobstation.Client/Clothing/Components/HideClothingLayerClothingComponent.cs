// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Client.Clothing.Components;

[RegisterComponent]
public sealed partial class HideClothingLayerClothingComponent : Component
{
    [DataField(required: true)]
    public HashSet<string> HiddenSlots = new();
}
