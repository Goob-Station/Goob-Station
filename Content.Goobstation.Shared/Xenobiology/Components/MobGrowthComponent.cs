// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This is used for mob growth between baby, adult etc...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MobGrowthComponent : Component
{
    /// <summary>
    /// What hunger threshold must be reached to grow?
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public float HungerRequired = 100f;

    /// <summary>
    /// How much hunger does growing consume?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GrowthCost = -75f;

    /// <summary>
    /// What is the mob's current growth stage?
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public string CurrentStage;

    [ViewVariables(VVAccess.ReadOnly)]
    public string FirstStage => Stages.Keys.FirstOrDefault() ?? string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsFirstStage => FirstStage == CurrentStage;

    /// <summary>
    /// A list of available stages.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public Dictionary<string, GrowthStageData> Stages = [];

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextGrowthTime;

    [DataField, AutoNetworkedField]
    public TimeSpan GrowthInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The base name of the entity, to be modified.
    /// </summary>
    [ViewVariables]
    public string BaseEntityName;

}
[Serializable, NetSerializable, DataDefinition]
public sealed partial class GrowthStageData
{
    [DataField]
    public string? NextStage { get; set; }

    [DataField]
    public string? Sprite { get; set; }

    [DataField]
    public string? DisplayName { get; set; } = string.Empty;
}
