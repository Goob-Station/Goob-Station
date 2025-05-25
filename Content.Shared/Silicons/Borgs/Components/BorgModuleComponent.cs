// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// This is used for modules that can be inserted into borgs
/// to give them unique abilities and attributes.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedBorgSystem))]
[AutoGenerateComponentState]
public sealed partial class BorgModuleComponent : Component
{
    /// <summary>
    /// The entity this module is installed into
    /// </summary>
    [DataField("installedEntity")]
    public EntityUid? InstalledEntity;

    public bool Installed => InstalledEntity != null;

    /// <summary>
    /// If true, this is a "default" module that cannot be removed from a borg.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public bool DefaultModule;
}

/// <summary>
/// Raised on a module when it is installed in order to add specific behavior to an entity.
/// </summary>
/// <param name="ChassisEnt"></param>
[ByRefEvent]
public readonly record struct BorgModuleInstalledEvent(EntityUid ChassisEnt);

/// <summary>
/// Raised on a module when it's uninstalled in order to
/// </summary>
/// <param name="ChassisEnt"></param>
[ByRefEvent]
public readonly record struct BorgModuleUninstalledEvent(EntityUid ChassisEnt);