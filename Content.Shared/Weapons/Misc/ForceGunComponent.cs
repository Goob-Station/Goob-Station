// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Misc;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ForceGunComponent : BaseForceGunComponent
{
    /// <summary>
    /// Maximum distance to throw entities.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("throwDistance"), AutoNetworkedField]
    public float ThrowDistance = 15f;

    [ViewVariables(VVAccess.ReadWrite), DataField("throwForce"), AutoNetworkedField]
    public float ThrowForce = 30f;

    /// <summary>
    /// The entity currently tethered.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("tethered"), AutoNetworkedField]
    public override EntityUid? Tethered { get; set; }

    [ViewVariables(VVAccess.ReadWrite), DataField("soundLaunch")]
    public SoundSpecifier? LaunchSound = new SoundPathSpecifier("/Audio/Weapons/soup.ogg")
    {
        Params = AudioParams.Default.WithVolume(5f),
    };
}