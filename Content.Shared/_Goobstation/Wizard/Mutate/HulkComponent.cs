using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Weapons.Ranged;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Mutate;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HulkComponent : Component
{
    /// <summary>
    /// If it's null, hulk is permanent
    /// </summary>
    [DataField]
    public float? Duration;

    [DataField, AutoNetworkedField]
    public float OldScale;

    [DataField, AutoNetworkedField]
    public bool LaserEyes = true;

    [DataField]
    public SoundSpecifier? SoundGunshot = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/laser_cannon.ogg");

    [DataField]
    public ProtoId<HitscanPrototype> ShotProto = "RedHeavyLaser";

    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo> OldCustomBaseLayers = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public Color OldSkinColor;

    [ViewVariables(VVAccess.ReadOnly)]
    public Color OldEyeColor;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextRoar = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<Color> NonHumanoidOldLayerData = new();

    /// <summary>
    /// Multiplier is actually this value + 1
    /// </summary>
    [DataField]
    public float FistDamageMultiplier = 7f;

    [DataField]
    public float MaxBonusFistDamage = 50f;

    [DataField]
    public DamageSpecifier? StructuralDamage;

    [DataField]
    public Color SkinColor = Color.FromHex("#4EDB53");

    [DataField]
    public Color EyeColor = Color.FromHex("#910C17");

    [DataField]
    public string BaseLayerExternal = "MobHumanoidMarkingMatchSkin";

    [DataField]
    public TimeSpan RoarDelay = TimeSpan.FromSeconds(0.5);

    [DataField]
    public List<LocId> Roars = new()
    {
        "hulk-roar-1",
        "hulk-roar-2",
        "hulk-roar-3",
        "hulk-roar-4",
        "hulk-roar-5",
    };
}
