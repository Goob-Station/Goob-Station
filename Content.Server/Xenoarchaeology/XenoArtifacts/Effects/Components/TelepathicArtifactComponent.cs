// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
///     Harmless artifact that broadcast "thoughts" to players nearby.
///     Thoughts are shown as popups and unique for each player.
/// </summary>
[RegisterComponent]
public sealed partial class TelepathicArtifactComponent : Component
{
    /// <summary>
    ///     Loc string ids of telepathic messages.
    ///     Will be randomly picked and shown to player.
    /// </summary>
    [DataField("messages")]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> Messages = default!;

    /// <summary>
    ///     Loc string ids of telepathic messages (spooky version).
    ///     Will be randomly picked and shown to player.
    /// </summary>
    [DataField("drastic")]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<string>? DrasticMessages;

    /// <summary>
    ///     Probability to pick drastic version of message.
    /// </summary>
    [DataField("drasticProb")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float DrasticMessageProb = 0.2f;

    /// <summary>
    ///     Radius in which player can receive artifacts messages.
    /// </summary>
    [DataField("range")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Range = 10f;
}