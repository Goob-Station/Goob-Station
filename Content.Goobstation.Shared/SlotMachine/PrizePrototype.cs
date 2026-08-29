using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SlotMachine;

/// <summary>
/// Prototype for the slotmachine and claw machine prizes and losses
/// </summary>
[Prototype]
public sealed partial class PrizePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    /// <summary>
    /// The change to win this prize
    /// </summary>
    [DataField]
    public required float Weight;

    /// <summary>
    /// The entity table to spawn when the prize is won
    /// </summary>
    [DataField]
    public EntityTableSelector? PrizeTable;

    /// <summary>
    /// The tag that the gambling machine must have for this prize to be a valid choice
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>>? Tags;

    [DataField]
    public LocId? WinMessage;

    [DataField]
    public SoundPathSpecifier WinSound = new ("/Audio/Effects/Arcade/win.ogg");
}
