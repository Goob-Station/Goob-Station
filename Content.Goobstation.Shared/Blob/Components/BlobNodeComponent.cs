// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Blob.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Blob.Components;
/// <remarks>
/// To add a new special blob tile you will need to change code in BlobNodeSystem and BlobTypedStorage.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobNodeComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Dictionary<ProtoId<BlobTilePrototype>, EntityUid> PlacedSpecials;

    [ViewVariables]
    public TimeSpan NextPulse;

    [DataField]
    public float PulseFrequency = 4f;

    [DataField]
    public float PulseRadius = 4f;

    [DataField]
    public float NodeRadiusLimit = 5f;

    [DataField]
    public float TilesRadiusLimit = 9f;
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

public sealed class BlobNodePulseEvent : EntityEventArgs;
