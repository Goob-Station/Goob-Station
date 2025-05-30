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
    /// Should this affect airborne mobs?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AffectAirborne;

    /// <summary>
    /// Modified mob friction while on FrictionContactsComponent
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MobFriction = 0.05f;

    /// <summary>
    /// Modified mob friction without input while on FrictionContactsComponent
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? MobFrictionNoInput = 0.05f;

    /// <summary>
    /// Modified mob acceleration while on FrictionContactsComponent
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MobAcceleration = 0.1f;
}
