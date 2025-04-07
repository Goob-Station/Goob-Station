// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Clothing.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SealableClothingRequiresPowerComponent : Component
{
    [DataField]
    public LocId NotPoweredPopup = "sealable-clothing-not-powered";

    [DataField]
    public LocId OpenSealedPanelFailPopup = "sealable-clothing-open-sealed-panel-fail";

    [DataField]
    public LocId ClosePanelFirstPopup = "sealable-clothing-close-panel-first";

    /// <summary>
    /// Movement speed on power end
    /// </summary>
    [DataField]
    public float MovementSpeedPenalty = 0.3f;

    [DataField, AutoNetworkedField]
    public bool IsPowered = false;

    /// <summary>
    /// Alert to show for suit power.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> SuitPowerAlert = "ModsuitPower";
}