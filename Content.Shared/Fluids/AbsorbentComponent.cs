// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Willhelm53 <97707302+Willhelm53@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Audio;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Fluids;

/// <summary>
/// For entities that can clean up puddles
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AbsorbentComponent : Component
{
    /// <summary>
    /// Used by the client to display a bar showing the reagents contained when held.
    /// Has to still be networked in case the item is given to someone who didn't see a mop in PVS.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Color, float> Progress = [];

    /// <summary>
    /// Name for solution container, that should be used for absorbed solution storage and as source of absorber solution.
    /// Default is 'absorbed'.
    /// </summary>
    [DataField]
    public string SolutionName = "absorbed";

    /// <summary>
    /// How much solution we can transfer in one interaction.
    /// </summary>
    [DataField("pickupAmount")]
    public FixedPoint2 PickupAmount = FixedPoint2.New(100);

    /// <summary>
    /// The effect spawned when the puddle fully evaporates.
    /// </summary>
    [DataField]
    public EntProtoId MoppedEffect = "PuddleSparkle";

    [DataField]
    public SoundSpecifier PickupSound = new SoundPathSpecifier("/Audio/Effects/Fluids/watersplash.ogg",
        AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation));

    [DataField]
    public SoundSpecifier TransferSound = new SoundPathSpecifier("/Audio/Effects/Fluids/slosh.ogg",
        AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(-3f));

    public static readonly SoundSpecifier DefaultTransferSound =
        new SoundPathSpecifier("/Audio/Effects/Fluids/slosh.ogg",
            AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(-3f));

    /// <summary>
    /// Marker that absorbent component owner should try to use 'absorber solution' to replace solution to be absorbed.
    /// Target solution will be simply consumed into container if set to false.
    /// </summary>
    [DataField]
    public bool UseAbsorberSolution = true;
}
