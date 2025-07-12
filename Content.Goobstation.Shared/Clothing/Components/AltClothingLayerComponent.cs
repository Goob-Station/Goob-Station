// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class AltClothingLayerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool AltStyle;

    [DataField(required: true)]
    public string DefaultLayer;

    [DataField(required: true)]
    public string AltLayer;

    [DataField(required: true)]
    public LocId ChangeToAltMessage;

    [DataField(required: true)]
    public LocId ChangeToDefaultMessage;
}
