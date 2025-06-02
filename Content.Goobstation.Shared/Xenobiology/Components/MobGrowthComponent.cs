// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

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

    /// <summary>
    /// A list of available stages, make sure to include the base stage.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public List<string> Stages = [];

}
