using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Blob.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Serializable]
public sealed partial class BlobTileComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    [ViewVariables]
    public Entity<BlobCoreComponent>? Core;

    [DataField]
    public bool ReturnCost = true;

    [DataField(required: true)]
    public BlobTileType BlobTileType = BlobTileType.Invalid;

    [DataField]
    public DamageSpecifier HealthOfPulse = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", -4 },
            { "Slash", -4 },
            { "Piercing", -4 },
            { "Heat", -4 },
            { "Cold", -4 },
            { "Shock", -4 },
        }
    };

    [DataField]
    public DamageSpecifier FlashDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Heat", 24 },
        }
    };
}

[Serializable]
public enum BlobTileType : byte
{
    Invalid, // invalid default value 0
    Normal,
    Strong,
    Reflective,
    Resource,
    /*
    Storage,
    Turret,
    */
    Node,
    Factory,
    Core,
}
