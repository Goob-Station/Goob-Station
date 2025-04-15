// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Content.Shared.DeviceNetwork.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.DeviceNetwork.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedNetworkConfiguratorSystem))]
public sealed partial class NetworkConfiguratorComponent : Component
{
    // AAAAA ALL OF THESE FAA
    /// <summary>
    /// Determines whether the configurator is in linking mode or list mode
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool LinkModeActive = true;

    /// <summary>
    /// The entity containing a <see cref="DeviceListComponent"/> this configurator is currently interacting with
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ActiveDeviceList { get; set; }

    /// <summary>
    /// The entity containing a <see cref="DeviceLinkSourceComponent"/> or <see cref="DeviceLinkSinkComponent"/> this configurator is currently interacting with.<br/>
    /// If this is set the configurator is in linking mode.
    /// </summary>
    // TODO handle device deletion
    public EntityUid? ActiveDeviceLink;

    /// <summary>
    /// The target device this configurator is currently linking with the <see cref="ActiveDeviceLink"/>
    /// </summary>
    // TODO handle device deletion
    public EntityUid? DeviceLinkTarget;

    /// <summary>
    /// The list of devices stored in the configurator
    /// </summary>
    [DataField]
    public Dictionary<string, EntityUid> Devices = new();

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(0.5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastUseAttempt;

    [DataField]
    public SoundSpecifier SoundNoAccess = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");

    [DataField]
    public SoundSpecifier SoundSwitchMode = new SoundPathSpecifier("/Audio/Machines/quickbeep.ogg");

    [DataField]
    public bool ShowLabel = true; // Shitmed - Starlight Abductors Change
}