using System.Collections;
using System.Diagnostics.Contracts;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Damage;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Blob;

#region BlobChemTypedStorage
[DataDefinition, Serializable, NetSerializable]
public abstract partial class BlobChemTypedStorage<T> : IEnumerable
{
    [DataField]
    public virtual T BlazingOil { get; set; } = default!;
    [DataField]
    public virtual T ReactiveSpines { get; set; }= default!;
    [DataField]
    public virtual T RegenerativeMateria { get; set; }= default!;
    [DataField]
    public virtual T ExplosiveLattice { get; set; }= default!;
    [DataField]
    public virtual T ElectromagneticWeb { get; set; }= default!;

    // Indexer to access fields via BlobChemType enumeration
    [Pure]
    public T this[BlobChemType type]
    {
        get => type switch
        {
            BlobChemType.BlazingOil => BlazingOil,
            BlobChemType.ReactiveSpines => ReactiveSpines,
            BlobChemType.RegenerativeMateria => RegenerativeMateria,
            BlobChemType.ExplosiveLattice => ExplosiveLattice,
            BlobChemType.ElectromagneticWeb => ElectromagneticWeb,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown chemical type: {type}")
        };
        set
        {
            switch (type)
            {
                case BlobChemType.BlazingOil:
                    BlazingOil = value;
                    break;
                case BlobChemType.ReactiveSpines:
                    ReactiveSpines = value;
                    break;
                case BlobChemType.RegenerativeMateria:
                    RegenerativeMateria = value;
                    break;
                case BlobChemType.ExplosiveLattice:
                    ExplosiveLattice = value;
                    break;
                case BlobChemType.ElectromagneticWeb:
                    ElectromagneticWeb = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unknown chemical type: {type}");
            }
        }
    }

    // Method for adding a value
    public void Add(BlobChemType key, T value)
    {
        this[key] = value;
    }

    // Realization IEnumerable
    public IEnumerator<KeyValuePair<BlobChemType, T>> GetEnumerator()
    {
        yield return new KeyValuePair<BlobChemType, T>(BlobChemType.BlazingOil, BlazingOil);
        yield return new KeyValuePair<BlobChemType, T>(BlobChemType.ReactiveSpines, ReactiveSpines);
        yield return new KeyValuePair<BlobChemType, T>(BlobChemType.RegenerativeMateria, RegenerativeMateria);
        yield return new KeyValuePair<BlobChemType, T>(BlobChemType.ExplosiveLattice, ExplosiveLattice);
        yield return new KeyValuePair<BlobChemType, T>(BlobChemType.ElectromagneticWeb, ElectromagneticWeb);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
#endregion

[DataDefinition, Serializable, NetSerializable]
public sealed partial class BlobChemColors : BlobChemTypedStorage<Color>
{

}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class BlobChemDamage : BlobChemTypedStorage<DamageSpecifier>
{

}
