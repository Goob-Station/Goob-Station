// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Blob.Prototypes;
using Content.Goobstation.Shared.Blob.Systems;
using Content.Goobstation.Shared.Blob.Systems.Core;
using Content.Goobstation.Shared.Blob.Systems.Observer;
using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Components;

[Access(typeof(SharedBlobCoreSystem), typeof(SharedBlobObserverSystem), typeof(SharedBlobTileSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobCoreComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Projection = default!;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Controller;

    [ViewVariables]
    public HashSet<EntityUid> BlobTiles = new();

    // TODO move this to ActionGrant
    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Actions = new();

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextAction = TimeSpan.Zero;

    [DataField]
    public FixedPoint2 MaxPoints = 250;

    /// <summary>
    /// TODO replace this with Currency when StoreSystem is predicted
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 Points;

    [ViewVariables]
    public ProtoId<BlobChemPrototype> CurrentChemical;

    [DataField]
    public ProtoId<BlobChemPrototype> StartingChemical;

    [DataField]
    public DamageSpecifier AttackDamage = new();

    [DataField]
    public float AttackRate = 0.5f;

    [DataField]
    public float GrowRate = 0.4f;

    [DataField]
    public bool CanSplit = true;

    [DataField]
    public FixedPoint2 AttackCost = 4;

    // TODO move this to actions
    [DataField]
    public FixedPoint2 BlobbernautCost = 60;

    [DataField]
    public FixedPoint2 SplitCoreCost = 400;

    [DataField]
    public FixedPoint2 SwapCoreCost = 200;

    [DataField]
    public FixedPoint2 SwapChemCost = 70;

    // TODO move this to ActionGrant
    [DataField]
    public List<EntProtoId> ActionPrototypes = new();

    [DataField]
    public List<ProtoId<BlobChemPrototype>> AvailableChemicals = new();

    [DataField]
    public ProtoId<AlertPrototype> HealthAlert = "BlobHealth";

    [DataField]
    public ProtoId<AlertPrototype> ResourceAlert = "BlobResource";

    [DataField]
    public EntProtoId<BlobProjectionComponent> ProjectionProtoId = "MobObserverBlob";

    [DataField]
    public EntProtoId MindRoleBlobPrototypeId = "MindRoleBlob";

    /// <summary>
    /// Tile that is placed when player clicks on an empty spot near another tile.
    /// </summary>
    [DataField]
    public ProtoId<BlobTilePrototype> GrowthTile = "MindRoleBlob";

    /// <summary>
    /// If specified, stops blob from growing too far away from nodes.
    /// </summary>
    [DataField]
    public float? TilesRadiusLimit = 9f;

    [DataField]
    public SoundSpecifier? GreetSoundNotification = new SoundPathSpecifier("/Audio/Effects/clang.ogg");

    [DataField]
    public SoundSpecifier? AttackSound = new SoundPathSpecifier("/Audio/Animals/Blob/blobattack.ogg");
}
