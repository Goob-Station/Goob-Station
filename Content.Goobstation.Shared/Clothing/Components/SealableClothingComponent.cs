// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Clothing.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Clothing.Components;

/// <summary>
///     Defines the clothing entity that can be sealed by <see cref="SealableClothingControlComponent"/>
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSealableClothingSystem))]
public sealed partial class SealableClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsSealed = false;

    [DataField, AutoNetworkedField]
    public TimeSpan SealingTime = TimeSpan.FromSeconds(1.75);

    [DataField]
    public LocId SealUpPopup = "sealable-clothing-seal-up";

    [DataField]
    public LocId SealDownPopup = "sealable-clothing-seal-down";

    [DataField]
    public SoundSpecifier SealUpSound = new SoundPathSpecifier("/Audio/Mecha/mechmove03.ogg");

    [DataField]
    public SoundSpecifier SealDownSound = new SoundPathSpecifier("/Audio/Mecha/mechmove03.ogg");
}