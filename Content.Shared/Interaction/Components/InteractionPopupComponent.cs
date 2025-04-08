// SPDX-FileCopyrightText: 2022 Júlio César Ueti <52474532+Mirino97@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Willhelm53 <97707302+Willhelm53@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Interaction.Components;

[RegisterComponent, Access(typeof(InteractionPopupSystem))]
public sealed partial class InteractionPopupComponent : Component
{
    /// <summary>
    /// Time delay between interactions to avoid spam.
    /// </summary>
    [DataField("interactDelay")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan InteractDelay = TimeSpan.FromSeconds(1.0);

    /// <summary>
    /// String will be used to fetch the localized message to be played if the interaction succeeds.
    /// Nullable in case none is specified on the yaml prototype.
    /// </summary>
    [DataField("interactSuccessString")]
    public string? InteractSuccessString;

    /// <summary>
    /// String will be used to fetch the localized message to be played if the interaction fails.
    /// Nullable in case no message is specified on the yaml prototype.
    /// </summary>
    [DataField("interactFailureString")]
    public string? InteractFailureString;

    /// <summary>
    /// Sound effect to be played when the interaction succeeds.
    /// Nullable in case no path is specified on the yaml prototype.
    /// </summary>
    [DataField("interactSuccessSound")]
    public SoundSpecifier? InteractSuccessSound;

    /// <summary>
    /// Sound effect to be played when the interaction fails.
    /// Nullable in case no path is specified on the yaml prototype.
    /// </summary>
    [DataField("interactFailureSound")]
    public SoundSpecifier? InteractFailureSound;

    /// <summary>
    /// a prototype that will spawn upon successful interaction (as planned only for special effects)
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId? InteractSuccessSpawn;

    /// <summary>
    /// a prototype that will spawn upon failure interaction (as planned only for special effects)
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId? InteractFailureSpawn;

    /// <summary>
    /// Chance that an interaction attempt will succeed.
    /// 1   = always play "success" popup and sound.
    /// 0.5 = 50% chance to play either success or failure popup and sound.
    /// 0   = always play "failure" popup and sound.
    /// </summary>
    [DataField("successChance")]
    public float SuccessChance = 1.0f; // Always succeed, unless specified otherwise on the yaml prototype.

    /// <summary>
    /// If set, shows a message to all surrounding players but NOT the current player.
    /// </summary>
    [DataField("messagePerceivedByOthers")]
    public string? MessagePerceivedByOthers;

    /// <summary>
    /// Will the sound effect be perceived by entities not involved in the interaction?
    /// </summary>
    [DataField("soundPerceivedByOthers")]
    public bool SoundPerceivedByOthers = true;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastInteractTime;

    /// <summary>
    /// If set to true, activate interactions will also trigger the component.
    /// </summary>
    [DataField]
    public bool OnActivate;
}