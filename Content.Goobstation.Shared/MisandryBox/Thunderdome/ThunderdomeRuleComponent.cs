using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MisandryBox.Thunderdome;

[DataDefinition]
public sealed partial class ThunderdomeWeaponLoadout
{
    [DataField(required: true)]
    public string Gear = string.Empty;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField]
    public string Description = string.Empty;

    [DataField(required: true)]
    public string Category = string.Empty;

    [DataField(required: true)]
    public string Sprite = string.Empty;
}

[RegisterComponent]
public sealed partial class ThunderdomeRuleComponent : Component
{
    [DataField]
    public MapId? ArenaMap;

    [DataField]
    public List<EntityUid> ArenaGrids = new();

    [DataField]
    public HashSet<NetEntity> Players = new();

    [DataField]
    public Dictionary<NetUserId, int> Kills = new();

    [DataField]
    public Dictionary<NetUserId, int> Deaths = new();

    [DataField]
    public bool Active;

    [DataField]
    public string Gear = "ThunderdomeBaseGear";

    [DataField]
    public ResPath MapPath = new("/Maps/_Goobstation/Nonstations/dm01-entryway.yml");

    [DataField]
    public List<ThunderdomeWeaponLoadout> WeaponLoadouts = new();

    [DataField]
    public TimeSpan CleanupInterval = TimeSpan.FromSeconds(25);

    [DataField]
    public TimeSpan NextCleanup;
}
