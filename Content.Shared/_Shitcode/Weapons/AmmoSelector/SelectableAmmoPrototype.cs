using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[Serializable, NetSerializable, DataDefinition]
[Prototype("selectableAmmo")]
public sealed partial class SelectableAmmoPrototype : IPrototype, ICloneable
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public SpriteSpecifier Icon;

    [DataField(required: true)]
    public string Desc;

    [DataField(required: true)]
    public EntProtoId ProtoId;

    [DataField]
    public Color? Color;

    [DataField]
    public float FireCost = 100f;

    [DataField]
    public SoundSpecifier? SoundGunshot;

    [DataField]
    public float FireRate = 8f;

    [DataField(customTypeSerializer: typeof(FlagSerializer<SelectableAmmoWeaponFlags>))]
    public int Flags = (int) SelectableAmmoFlags.ChangeWeaponFireCost;

    public object Clone()
    {
        return new SelectableAmmoPrototype
        {
            ID = ID,
            Icon = Icon,
            Desc = Desc,
            ProtoId = ProtoId,
            Color = Color,
            FireCost = FireCost,
            Flags = Flags,
            FireRate = FireRate,
            SoundGunshot = SoundGunshot,
        };
    }
}

public sealed class SelectableAmmoWeaponFlags;

[Serializable, NetSerializable]
[Flags, FlagsFor(typeof(SelectableAmmoWeaponFlags))]
public enum SelectableAmmoFlags
{
    None = 0,
    ChangeWeaponFireCost = 1 << 0,
    ChangeWeaponFireSound = 1 << 1,
    ChangeWeaponFireRate = 1 << 2,
    All = ~None,
}
