// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
///     Responsible for handling recharging a basic entity ammo provider over time.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class RechargeBasicEntityAmmoComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("rechargeCooldown")]
    [AutoNetworkedField]
    public float RechargeCooldown = 1.5f;

    [DataField("rechargeSound")]
    [AutoNetworkedField]
    public SoundSpecifier? RechargeSound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg")
    {
        Params = AudioParams.Default.WithVolume(-5f)
    };

    [ViewVariables(VVAccess.ReadWrite),
     DataField("nextCharge", customTypeSerializer:typeof(TimeOffsetSerializer)),
    AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan? NextCharge;

    [DataField, AutoNetworkedField]
    public bool ShowExamineText = true;
}