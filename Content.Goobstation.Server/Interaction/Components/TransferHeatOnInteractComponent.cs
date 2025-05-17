// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Interaction.Components;

/// <summary>
/// Equalizes temperature between two entities on interact - Hug your friends!
/// In the future, I want moths to be able to roast people in a swarm
/// like how bees defend their hive.
/// </summary>
[RegisterComponent]
public sealed partial class TransferHeatOnInteractComponent : Component
{
    /// <summary>
    /// The amount of the host entity heat to transfer per interaction.
    /// </summary>
    [DataField]
    public float TransferRatio = 0.45f;

    /// <summary>
    /// If set to true, activate interactions will also trigger the component.
    /// </summary>
    [DataField]
    public bool OnActivate;

    /// <summary>
    /// Should this entity transfer firestacks on interaction?
    /// </summary>
    /// <remarks>
    /// This effectively allows you to play 'hot potato' with fire stacks.
    /// Lol.
    /// </remarks>
    [DataField]
    public bool TransferFireStacks = true;

    /// <summary>
    /// Time delay between interactions to avoid spam.
    /// </summary>
    [DataField]
    public TimeSpan InteractDelay = TimeSpan.FromSeconds(1.0);

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastInteractTime;

}
