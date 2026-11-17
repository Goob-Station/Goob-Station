using Content.Shared._EinsteinEngines.Language;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Modules;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModEmpWardComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModSignRadioComponent : Component
{
    [DataField]
    public List<string> Languages = new();

    [DataField]
    public ProtoId<LanguagePrototype>? TranslateTo = "TauCetiBasic";
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModJumpJetModuleComponent : Component
{
    [DataField]
    public float Distance = 6f;

    [DataField]
    public float Speed = 12f;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/rocket_launcher.ogg");
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModJointTorsionModuleComponent : Component
{
    [DataField]
    public float ChargePerTile = 0.4f;

    [DataField]
    public float MaxTilesPerUpdate = 2f;

    [ViewVariables]
    public MapCoordinates? LastPosition;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModPlasmaStabilizerModuleComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModHatStabilizerModuleComponent : Component
{
    [DataField]
    public string SlotId = "mod-hat";

    [DataField]
    public EntityWhitelist? Whitelist;

    [ViewVariables]
    public EntityUid? Helmet;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModHatHolderComponent : Component
{
    [DataField, AutoNetworkedField]
    public string SlotId = "mod-hat";
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModRecyclerModuleComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public float Range = 1.2f;

    [DataField]
    public float PullSpeed = 2.5f;

    [DataField]
    public float CatchDistance = 0.3f;

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(0.25);

    [DataField]
    public EntProtoId? Product;

    [DataField]
    public EntProtoId UnloadAction = "ActionModRecyclerUnload";

    [DataField, AutoNetworkedField]
    public EntityUid? UnloadActionEntity;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/trashbag1.ogg");

    [DataField, AutoNetworkedField]
    public Dictionary<string, int> Materials = new();

    [DataField, AutoNetworkedField]
    public int Products;

    [ViewVariables]
    public TimeSpan NextCollect;
}
public sealed partial class ModRecyclerUnloadActionEvent : Content.Shared.Actions.InstantActionEvent;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModMagneticHarnessComponent : Component
{
    [DataField]
    public List<string> Slots = new() { "back", "suitstorage" };

    [DataField]
    public float Range = 7f;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.5);

    [DataField]
    public TimeSpan PullTime = TimeSpan.FromSeconds(1);

    [DataField]
    public float PullSpeed = 1.2f;

    [DataField]
    public float LaunchSpeed = 12f;

    [DataField]
    public TimeSpan GiveUpTime = TimeSpan.FromSeconds(4);

    [DataField]
    public float CatchDistance = 0.6f;

    [DataField]
    public SoundSpecifier LaunchSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/Modsuit/gravhit.ogg", AudioParams.Default.WithVolume(-4f));

    [DataField]
    public SoundSpecifier CatchSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_end.ogg");
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ModMagnetizedGunComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Wearer;

    [DataField, AutoNetworkedField]
    public string Slot = "back";

    [DataField, AutoNetworkedField]
    public TimeSpan? LooseSince;

    [DataField, AutoNetworkedField]
    public bool Launched;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModProjectileDampenerModuleComponent : Component
{
    [DataField]
    public float Radius = 1.5f;

    [DataField]
    public float SpeedMultiplier = 0.04f;

    [DataField]
    public float DamageMultiplier = 0.75f;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModDampenerFieldComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;

    [DataField, AutoNetworkedField]
    public float Radius = 2.5f;

    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 0.04f;

    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 0.75f;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModDampenedProjectileComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Field;

    [DataField, AutoNetworkedField]
    public float OriginalSpeed;

    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 1f;
}

[Serializable, NetSerializable]
public enum ModShootingMode : byte
{
    Off,
    Stormtrooper,
    Sharpshooter,
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModShootingAssistantModuleComponent : Component
{
    [DataField, AutoNetworkedField]
    public ModShootingMode Mode = ModShootingMode.Off;

    [DataField]
    public EntProtoId CycleAction = "ActionModShootingMode";

    [DataField, AutoNetworkedField]
    public EntityUid? CycleActionEntity;

    [DataField]
    public float StormtrooperFireRate = 1.5f;

    [DataField]
    public float StormtrooperMinSpread = 15f;

    [DataField]
    public float StormtrooperMaxSpread = 25f;

    [DataField]
    public float SharpshooterSpreadMultiplier = 0.35f;

    [DataField]
    public float SharpshooterProjectileSpeed = 1.3f;

    [DataField]
    public float SharpshooterDamage = 1.15f;

    [DataField]
    public float SharpshooterMoveSpeed = 0.75f;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModShootingAssistantComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;

    [DataField, AutoNetworkedField]
    public ModShootingMode Mode;
}

public sealed partial class ModShootingModeActionEvent : Content.Shared.Actions.InstantActionEvent;
