// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Damage.Prototypes;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Overlays;

/// <summary>
/// This component allows you to see health bars above damageable mobs.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class ShowHealthBarsComponent : Component
{
    /// <summary>
    /// Displays health bars of the damage containers.
    /// </summary>
    [DataField, AutoNetworkedField] // Shitmed Change
    public List<ProtoId<DamageContainerPrototype>> DamageContainers = new()
    {
        "Biological"
    };

    [DataField]
    public ProtoId<HealthIconPrototype>? HealthStatusIcon = "HealthIconFine";
}
