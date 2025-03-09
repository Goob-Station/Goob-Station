using System.Collections;
using System.Diagnostics.Contracts;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Blob;

#region BlobTypedStorage
[DataDefinition]
public abstract partial class BlobTypedStorage<T> : IEnumerable<KeyValuePair<BlobTileType, T>>
{
    [DataField]
    public virtual T Core { get; set; } = default!;
    [DataField]
    public virtual T Invalid  { get; set; } = default!;
    [DataField]
    public virtual T Resource  { get; set; } = default!;
    [DataField]
    public virtual T Factory  { get; set; } = default!;
    [DataField]
    public virtual T Node  { get; set; } = default!;
    [DataField]
    public virtual T Reflective  { get; set; } = default!;
    [DataField]
    public virtual T Strong  { get; set; } = default!;
    [DataField]
    public virtual T Normal { get; set; } = default!;
    /*
    [DataField]
    public virtual T Storage  { get; set; }
    [DataField]
    public virtual T Turret { get; set; }
*/
    // Method for accessing fields through the indexer
    [Pure]
    public T this[BlobTileType type]
    {
        get => type switch
        {
            BlobTileType.Core => Core,
            BlobTileType.Invalid => Invalid,
            BlobTileType.Resource => Resource,
            BlobTileType.Factory => Factory,
            BlobTileType.Node => Node,
            BlobTileType.Reflective => Reflective,
            BlobTileType.Strong => Strong,
            BlobTileType.Normal => Normal,
            /*
            BlobTileType.Storage => Storage,
            BlobTileType.Turret => Turret,
            */
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tile type: {type}")
        };
        set
        {
            switch (type)
            {
                case BlobTileType.Core:
                    Core = value;
                    break;
                case BlobTileType.Invalid:
                    Invalid = value;
                    break;
                case BlobTileType.Resource:
                    Resource = value;
                    break;
                case BlobTileType.Factory:
                    Factory = value;
                    break;
                case BlobTileType.Node:
                    Node = value;
                    break;
                case BlobTileType.Reflective:
                    Reflective = value;
                    break;
                case BlobTileType.Strong:
                    Strong = value;
                    break;
                case BlobTileType.Normal:
                    Normal = value;
                    break;
                /*
                case BlobTileType.Storage:
                    Storage = value;
                    break;
                case BlobTileType.Turret:
                    Turret = value;
                    break;
                    */
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tile type: {type}");
            }
        }
    }

    public void Add(BlobTileType key, T value)
    {
        this[key] = value;
    }

    public IEnumerator<KeyValuePair<BlobTileType, T>> GetEnumerator()
    {
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Core, Core);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Invalid, Invalid);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Resource, Resource);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Factory, Factory);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Node, Node);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Reflective, Reflective);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Strong, Strong);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Normal, Normal);
        /*
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Storage, Storage);
        yield return new KeyValuePair<BlobTileType, T>(BlobTileType.Turret, Turret);
        */
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
#endregion

[DataDefinition]
public sealed partial class BlobTileCosts : BlobTypedStorage<FixedPoint2>
{
}

[DataDefinition]
public sealed partial class BlobTileProto : BlobTypedStorage<EntProtoId<BlobTileComponent>>
{
}
