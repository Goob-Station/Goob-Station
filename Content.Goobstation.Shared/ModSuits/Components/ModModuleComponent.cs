using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
[Access(typeof(ModSuitSystem))]
public sealed partial class ModModuleComponent : Component
{
    [DataField("moduleType")]
    public ModModuleType Type = ModModuleType.Passive;

    [DataField]
    public int Complexity;

    [DataField]
    public float IdlePower;

    [DataField]
    public float ActivePower;

    [DataField]
    public float UsePower;

    [DataField]
    public TimeSpan Cooldown;

    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUse;

    [DataField, AutoNetworkedField]
    public bool Removable = true;

    [DataField]
    public EntityWhitelist? Incompatible;

    [DataField]
    public List<string> RequiredParts = new();

    [DataField]
    public ModModuleAllowFlags AllowFlags = ModModuleAllowFlags.None;

    [DataField]
    public EntProtoId? Action = "ActionModModule";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public bool Pinned = true;

    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField, AutoNetworkedField]
    public EntityUid? Suit;
}

[Serializable, NetSerializable]
public enum ModModuleType : byte
{
    Passive,
    Usable,
    Toggle,
    Active,
}

[Flags]
[Serializable, NetSerializable]
public enum ModModuleAllowFlags : byte
{
    None = 0,
    Inactive = 1 << 0,
    Unworn = 1 << 1,
    Incapacitated = 1 << 2,
}
