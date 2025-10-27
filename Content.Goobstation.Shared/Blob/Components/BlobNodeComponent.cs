// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Blob.Components;
/// <remarks>
/// To add a new special blob tile you will need to change code in BlobNodeSystem and BlobTypedStorage.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobNodeComponent : Component
{
    [DataField]
    public float PulseFrequency = 4f;

    [DataField]
    public float PulseRadius = 4f;

    public float NextPulse = 0;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? BlobResource = null;
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? BlobFactory = null;
    /*
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? BlobStorage = null;
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? BlobTurret = null;
    */
}

public sealed class BlobTileGetPulseEvent : HandledEntityEventArgs
{

}

[Serializable, NetSerializable]
public sealed partial class BlobMobGetPulseEvent : EntityEventArgs
{
    public NetEntity BlobEntity { get; set; }
}

/// <summary>
/// Event raised on all special tiles of Blob Node on pulse.
/// </summary>
public sealed class BlobSpecialGetPulseEvent : EntityEventArgs;

/// <summary>
/// Event
/// </summary>
public sealed class BlobNodePulseEvent : EntityEventArgs;