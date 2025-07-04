// SPDX-FileCopyrightText: 2023 KISS <59531932+YuriyKiss@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Yurii Kis <yurii.kis@smartteksas.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

[NetworkedComponent, RegisterComponent]
[AutoGenerateComponentState]
[Access(typeof(FrictionContactsSystem))]
public sealed partial class FrictionContactsComponent : Component
{
    /// <summary>
    /// Modified mob friction while on FrictionContactsComponent
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float MobFriction = 0.5f;

    /// <summary>
    /// Modified mob friction without input while on FrictionContactsComponent
    /// </summary>
    [AutoNetworkedField]
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MobFrictionNoInput = 0.05f;

    /// <summary>
    /// Modified mob acceleration while on FrictionContactsComponent
    /// </summary>
    [AutoNetworkedField]
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MobAcceleration = 2.0f;
}