// SPDX-FileCopyrightText: 2023 PixelTK <85175107+PixelTheKermit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Nutrition.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Sericulture;

/// <summary>
/// Should be applied to any mob that you want to be able to produce any material with an action and the cost of hunger.
/// TODO: Probably adjust this to utilize organs?
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSericultureSystem)), AutoGenerateComponentState]
public sealed partial class SericultureComponent : Component
{
    /// <summary>
    /// The text that pops up whenever sericulture fails for not having enough hunger.
    /// </summary>
    [DataField("popupText")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public string PopupText = "sericulture-failure-hunger";

    /// <summary>
    /// What will be produced at the end of the action.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public EntProtoId EntityProduced;

    /// <summary>
    /// The entity needed to actually preform sericulture. This will be granted (and removed) upon the entity's creation.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public EntProtoId Action;

    [AutoNetworkedField]
    [DataField("actionEntity")]
    public EntityUid? ActionEntity;

    /// <summary>
    /// How long will it take to make.
    /// </summary>
    [DataField("productionLength")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float ProductionLength = 3f;

    /// <summary>
    /// This will subtract (not add, don't get this mixed up) from the current hunger of the mob doing sericulture.
    /// </summary>
    [DataField("hungerCost")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float HungerCost = 5f;

    /// <summary>
    /// The lowest hunger threshold that this mob can be in before it's allowed to spin silk.
    /// </summary>
    [DataField("minHungerThreshold")]
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public HungerThreshold MinHungerThreshold = HungerThreshold.Okay;
}