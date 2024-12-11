using Content.Server.NPC.Components;
using Content.Server.WhiteDream.BloodCult.RendingRunePlacement;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Gamerule;

[RegisterComponent]
public sealed partial class BloodCultRuleComponent : Component
{
    [DataField]
    public ProtoId<NpcFactionPrototype> NanoTrasenFaction = "NanoTrasen";

    [DataField]
    public ProtoId<NpcFactionPrototype> BloodCultFaction = "GeometerOfBlood";

    [DataField]
    public EntProtoId HarvesterPrototype = "ConstructHarvester";

    [DataField]
    public Color EyeColor = Color.FromHex("#f80000");

    [DataField]
    public int ReadEyeThreshold = 5;

    [DataField]
    public int PentagramThreshold = 8;

    [DataField]
    public int RendingRunePlacementsAmount = 3;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool LeaderSelected;

    /// <summary>
    ///     If no rending rune markers were placed on the map, players will be able to place these runes anywhere on the map
    ///     but no more than <see cref="RendingRunePlacementsAmount">total available</see>.
    /// </summary>
    [DataField]
    public bool EmergencyMarkersMode;

    public int EmergencyMarkersCount;

    /// <summary>
    ///     The entityUid of body which should be sacrificed.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? OfferingTarget;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? CultLeader;

    [ViewVariables(VVAccess.ReadOnly)]
    public CultStage Stage = CultStage.Start;

    public CultWinCondition WinCondition = CultWinCondition.Draw;

    public List<Entity<BloodCultistComponent>> Cultists = new();

    public List<Entity<ConstructComponent>> Constructs = new();
}

public enum CultWinCondition : byte
{
    Draw,
    Win,
    Failure
}

public enum CultStage : byte
{
    Start,
    RedEyes,
    Pentagram
}

public sealed class BloodCultNarsieSummoned : EntityEventArgs;
