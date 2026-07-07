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
using Content.Shared.Explosion;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobCoreComponent : Component
{
    #region Live Data

    [ViewVariables]
    public EntityUid? Observer = default!;

    [ViewVariables]
    public HashSet<EntityUid> BlobTiles = [];

    [ViewVariables]
    public List<EntityUid> Actions = [];

    [ViewVariables]
    public TimeSpan NextAction = TimeSpan.Zero;

    [ViewVariables]
    public BlobChemType CurrentChem = BlobChemType.ReactiveSpines;

    #endregion

    #region Balance

    [DataField]
    public FixedPoint2 CoreBlobTotalHealth = 400;

    [DataField]
    public float StartingMoney = 250f; // enough for 2 resource nodes and a bit of defensive action

    [DataField]
    public float AttackRate = 0.3f;

    [DataField]
    public float GrowRate = 0.1f;

    [DataField]
    public bool CanSplit = true;

    #endregion

    #region Damage Specifiers

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public BlobChemDamage ChemDamageDict { get; set; } = new()
    {
        {
            BlobChemType.BlazingOil, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Heat", 15 },
                    { "Structural", 150 },
                }
            }
        },
        {
            BlobChemType.ReactiveSpines, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Blunt", 8 },
                    { "Slash", 8 },
                    { "Piercing", 8 },
                    { "Structural", 150 },
                }
            }
        },
        {
            BlobChemType.ExplosiveLattice, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Heat", 5 },
                    { "Structural", 150 },
                }
            }
        },
        {
            BlobChemType.ElectromagneticWeb, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 150 },
                    { "Shock", 18 },
                },
            }
        },
        {
            BlobChemType.RegenerativeMateria, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 120 },
                    { "Poison", 15 },
                }
            }
        },
        {
            BlobChemType.ComatoseFiber, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 150 },
                    { "Asphyxiation", 22 },
                }
            }
        },
        {
            BlobChemType.ChainCoating, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 150 },
                    { "Blunt", 12 },
                    { "Slash", 12 },
                }
            }
        },
        {
            BlobChemType.SinewyTendons, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 150 },
                    { "Blunt", -8 },
                    { "Slash", -8 },
                    { "Piercing", -8 },
                    { "Poison", -8 },
                    { "Heat", -8 },
                    { "Cold", -8 },
                    { "Asphyxiation", -8 },
                }
            }
        },
        {
            BlobChemType.CorrosiveSlime, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 320 },
                    { "Caustic", 13 },
                    { "Cellular", 2}
                }
            }
        },
        {
            BlobChemType.CryogenicPoison, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 100 },
                    { "Cold", 16 },
                    { "Slash", 4}
                }
            }
        },
    };

    #endregion

    #region Blob Chems

    [ViewVariables]
    public readonly BlobChemColors ChemСolors = new()
    {
        {BlobChemType.ReactiveSpines, Color.FromHex("#637b19")},
        {BlobChemType.BlazingOil, Color.FromHex("#937000")},
        {BlobChemType.RegenerativeMateria, Color.FromHex("#441e59")},
        {BlobChemType.ExplosiveLattice, Color.FromHex("#6e2a00")},
        {BlobChemType.ElectromagneticWeb, Color.FromHex("#0d7777")},
        {BlobChemType.ComatoseFiber, Color.FromHex("#191978")},
        {BlobChemType.ChainCoating, Color.FromHex("#3b3b3b")},
        {BlobChemType.SinewyTendons, Color.FromHex("#690f53")},
        {BlobChemType.CorrosiveSlime, Color.FromHex("#9cae6b")},
        {BlobChemType.CryogenicPoison, Color.FromHex("#5282ae")},
    };

    [DataField]
    public BlobChemType DefaultChem = BlobChemType.ReactiveSpines;

    #endregion

    #region Blob Costs

    [DataField]
    public int ResourceBlobsTotal;

    [DataField]
    public FixedPoint2 AttackCost = 4;

    [DataField]
    public BlobTileCosts BlobTileCosts = new()
    {
        {BlobTileType.Core, 0},
        {BlobTileType.Invalid, 0},
        {BlobTileType.Resource, 60},
        {BlobTileType.Factory, 80},
        {BlobTileType.Node, 50},
        {BlobTileType.Reflective, 15},
        {BlobTileType.Strong, 15},
        {BlobTileType.Normal, 6},
        /*
        {BlobTileType.Storage, 50},
        {BlobTileType.Turret, 75},*/
    };

    [DataField]
    public Dictionary<BlobTileType, Dictionary<BlobChemType, FixedPoint2>> BlobTileCostsByChem = new()
    {
        [ BlobTileType.Strong ] = new()
        {
            { BlobChemType.ChainCoating, 10 },
            { BlobChemType.SinewyTendons, 25 },
        },
        [ BlobTileType.Reflective ] = new()
        {
            { BlobChemType.ChainCoating, 10 },
            { BlobChemType.SinewyTendons, 20 },
        },
        [ BlobTileType.Factory ] = new()
        {
            { BlobChemType.ChainCoating, 100 },
            { BlobChemType.SinewyTendons, 60 },
        },
        [ BlobTileType.Resource ] = new()
        {
            { BlobChemType.ChainCoating, 85 },
            { BlobChemType.SinewyTendons, 40 },
        },
        [ BlobTileType.Node ] = new()
        {
            { BlobChemType.CorrosiveSlime, 100 },
        }
    };

    [DataField]
    public FixedPoint2 BlobbernautCost = 60;

    [DataField]
    public FixedPoint2 SplitCoreCost = 400;

    [DataField]
    public FixedPoint2 SwapCoreCost = 200;

    [DataField]
    public FixedPoint2 SwapChemCost = 70;

    #endregion

    #region Blob Ranges

    [DataField]
    public float NodeRadiusLimit = 5f;

    [DataField]
    public float TilesRadiusLimit = 9f;

    #endregion

    #region Prototypes

    [DataField]
    public BlobTileProto TilePrototypes = new()
    {
        {BlobTileType.Resource, "ResourceBlobTile"},
        {BlobTileType.Factory, "FactoryBlobTile"},
        {BlobTileType.Node, "NodeBlobTile"},
        {BlobTileType.Reflective, "ReflectiveBlobTile"},
        {BlobTileType.Strong, "StrongBlobTile"},
        {BlobTileType.Normal, "NormalBlobTile"},
        {BlobTileType.Invalid, "NormalBlobTile"}, // wtf
        //{BlobTileType.Storage, "StorageBlobTile"},
        //{BlobTileType.Turret, "TurretBlobTile"},
        {BlobTileType.Core, "CoreBlobTile"},
    };

    [DataField(required: true)]
    public List<EntProtoId> ActionPrototypes = [];

    [DataField]
    public ProtoId<ExplosionPrototype> BlobExplosive = "Blob";

    [DataField]
    public EntProtoId<BlobObserverComponent> ObserverBlobPrototype = "MobObserverBlob";

    [DataField]
    public EntProtoId MindRoleBlobPrototypeId = "MindRoleBlob";

    #endregion

    #region Sounds

    [DataField]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/Effects/clang.ogg");

    [DataField]
    public SoundSpecifier AttackSound = new SoundPathSpecifier("/Audio/Animals/Blob/blobattack.ogg");

    #endregion
}

[Serializable, NetSerializable]
public enum BlobChemType : byte
{
    BlazingOil,
    ReactiveSpines,
    RegenerativeMateria,
    ExplosiveLattice,
    ElectromagneticWeb,
    ComatoseFiber,
    SinewyTendons,
    ChainCoating,
    CorrosiveSlime,
    CryogenicPoison
}
