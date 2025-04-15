// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.CartridgeLoader;

/// <summary>
/// This is used for defining values used for displaying in the program ui in yaml
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CartridgeComponent : Component
{
    [DataField]
    public EntityUid? LoaderUid;

    [DataField(required: true)]
    public LocId ProgramName = "default-program-name";

    [DataField]
    public SpriteSpecifier? Icon;

    [AutoNetworkedField]
    public InstallationStatus InstallationStatus = InstallationStatus.Cartridge;
}

[Serializable, NetSerializable]
public enum InstallationStatus
{
    Cartridge,
    Installed,
    Readonly
}